using System.Collections.Generic;
using System.IO;
using Stella.Level04;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Stella.EditorTools
{
    /// <summary>
    /// Creates the placeholder scene through Unity APIs so every serialized reference is valid.
    /// It only runs automatically when the Level 4 scene does not exist yet.
    /// </summary>
    [InitializeOnLoad]
    public static class Level04SceneBuilder
    {
        public const string SceneAssetPath = "Assets/_Stella/Scenes/Level04_KrikGreeting.unity";

        private static readonly Color PageBackground = new Color(0.025f, 0.055f, 0.10f, 1f);
        private static readonly Color PanelBackground = new Color(0.07f, 0.12f, 0.19f, 0.96f);
        private static readonly Color DialogueBackground = new Color(0.095f, 0.17f, 0.25f, 1f);
        private static readonly Color PrimaryButton = new Color(0.13f, 0.64f, 0.73f, 1f);
        private static readonly Color LightText = new Color(0.91f, 0.97f, 1f, 1f);

        static Level04SceneBuilder()
        {
            EditorApplication.delayCall += BuildSceneIfMissing;
        }

        [MenuItem("Stella/Build Level 4 Placeholder Scene")]
        public static void BuildScene()
        {
            EnsureFolder("Assets/_Stella");
            EnsureFolder("Assets/_Stella/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Level04_KrikGreeting";

            GameObject sceneRoot = new GameObject("Level04_KrikGreeting");
            Level04Controller controller = sceneRoot.AddComponent<Level04Controller>();

            CreateEventSystem(sceneRoot.transform);
            Canvas canvas = CreateCanvas(sceneRoot.transform);
            CreateBackground(canvas.transform);

            BattlePanelView battlePanel = CreateTopRegion(canvas.transform);
            List<CrystalView> crystalViews;
            Button doneButton;
            GameObject retryPanel;
            Button retryButton;
            CreateBottomRegion(
                canvas.transform,
                controller,
                out crystalViews,
                out doneButton,
                out retryPanel,
                out retryButton);

            Button stellaStartsButton;
            Button krikStartsButton;
            GameObject startingChoicePanel = CreateStartingChoice(
                canvas.transform,
                out stellaStartsButton,
                out krikStartsButton);

            controller.Configure(
                battlePanel,
                crystalViews,
                doneButton,
                startingChoicePanel,
                stellaStartsButton,
                krikStartsButton,
                retryPanel,
                retryButton);

            EditorSceneManager.SaveScene(scene, SceneAssetPath);
            AddSceneToBuildSettings();

            // The Hub template's sample is removed only after the real Level 4 scene is safe on disk.
            if (AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.DeleteAsset("Assets/Scenes");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created Stella Level 4 placeholder scene at " + SceneAssetPath);
        }

        public static void BuildSceneFromCommandLine()
        {
            BuildScene();
        }

        private static void BuildSceneIfMissing()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneAssetPath) == null)
            {
                BuildScene();
            }
        }

        private static Canvas CreateCanvas(Transform parent)
        {
            GameObject canvasObject = new GameObject(
                "Level04Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private static void CreateEventSystem(Transform parent)
        {
            GameObject eventSystem = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule));
            eventSystem.transform.SetParent(parent, false);
        }

        private static void CreateBackground(Transform parent)
        {
            GameObject background = CreatePanel("Background", parent, PageBackground);
            Stretch(background.GetComponent<RectTransform>());
            background.transform.SetAsFirstSibling();
        }

        private static BattlePanelView CreateTopRegion(Transform parent)
        {
            GameObject topRegion = CreatePanel("TopRegion_30Percent", parent, PanelBackground);
            RectTransform topRect = topRegion.GetComponent<RectTransform>();
            SetAnchors(topRect, new Vector2(0.04f, 0.68f), new Vector2(0.96f, 0.96f));

            HorizontalLayoutGroup layout = topRegion.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 18, 18);
            layout.spacing = 28f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            GameObject card = CreatePanel("ActiveBattleCard", topRegion.transform, new Color(0.30f, 0.36f, 0.48f, 1f));
            LayoutElement cardLayout = card.AddComponent<LayoutElement>();
            cardLayout.minWidth = 210f;
            cardLayout.preferredWidth = 240f;
            cardLayout.flexibleWidth = 0f;
            Text cardLabel = CreateText("BattleCardLabel", card.transform, "RITUAL", 34, TextAnchor.MiddleCenter);
            Stretch(cardLabel.rectTransform, 16f);

            GameObject dialogue = CreatePanel("DialoguePanel", topRegion.transform, DialogueBackground);
            LayoutElement dialogueLayout = dialogue.AddComponent<LayoutElement>();
            dialogueLayout.minWidth = 300f;
            dialogueLayout.flexibleWidth = 1f;
            Text dialogueText = CreateText(
                "DialogueText",
                dialogue.transform,
                "Who should begin the Krik greeting?",
                32,
                TextAnchor.MiddleLeft);
            Stretch(dialogueText.rectTransform, 36f);
            dialogueText.horizontalOverflow = HorizontalWrapMode.Wrap;
            dialogueText.verticalOverflow = VerticalWrapMode.Truncate;

            BattlePanelView panelView = topRegion.AddComponent<BattlePanelView>();
            panelView.Configure(card.GetComponent<Image>(), cardLabel, dialogueText);
            return panelView;
        }

        private static void CreateBottomRegion(
            Transform parent,
            Level04Controller controller,
            out List<CrystalView> crystalViews,
            out Button doneButton,
            out GameObject retryPanel,
            out Button retryButton)
        {
            GameObject bottomRegion = CreatePanel("BottomRegion_70Percent", parent, PanelBackground);
            RectTransform bottomRect = bottomRegion.GetComponent<RectTransform>();
            SetAnchors(bottomRect, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.65f));

            VerticalLayoutGroup layout = bottomRegion.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 30, 24);
            layout.spacing = 22f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            GameObject gridObject = new GameObject(
                "CrystalGrid_2x10",
                typeof(RectTransform),
                typeof(GridLayoutGroup),
                typeof(LayoutElement),
                typeof(ResponsiveCrystalGrid));
            gridObject.transform.SetParent(bottomRegion.transform, false);

            LayoutElement gridLayoutElement = gridObject.GetComponent<LayoutElement>();
            gridLayoutElement.flexibleHeight = 1f;
            gridLayoutElement.minHeight = 220f;

            GridLayoutGroup gridLayout = gridObject.GetComponent<GridLayoutGroup>();
            gridLayout.childAlignment = TextAnchor.MiddleCenter;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 10;
            gridLayout.spacing = new Vector2(12f, 12f);

            crystalViews = new List<CrystalView>();
            for (int index = 0; index < KrikGreetingGame.StartingCrystalCount; index++)
            {
                int crystalNumber = index + 1;
                Button crystalButton = CreateButton(
                    "Crystal_" + crystalNumber.ToString("00"),
                    gridObject.transform,
                    "◆\n" + crystalNumber.ToString("00"),
                    new Color(0.20f, 0.82f, 0.98f, 1f),
                    22);

                CrystalView crystalView = crystalButton.gameObject.AddComponent<CrystalView>();
                Text label = crystalButton.GetComponentInChildren<Text>();
                crystalView.Configure(
                    index,
                    crystalButton.GetComponent<Image>(),
                    crystalButton,
                    label,
                    controller.HandleCrystalPressed);
                crystalViews.Add(crystalView);
            }

            GameObject actionRow = new GameObject("ActionRow", typeof(RectTransform), typeof(LayoutElement));
            actionRow.transform.SetParent(bottomRegion.transform, false);
            LayoutElement actionLayout = actionRow.GetComponent<LayoutElement>();
            actionLayout.minHeight = 82f;
            actionLayout.preferredHeight = 82f;

            doneButton = CreateButton("DoneButton", actionRow.transform, "DONE / CONFIRM", PrimaryButton, 28);
            SetCenteredSize(doneButton.GetComponent<RectTransform>(), new Vector2(360f, 76f));
            doneButton.interactable = false;

            retryPanel = CreatePanel("RetryPanel", bottomRegion.transform, new Color(0.025f, 0.055f, 0.10f, 0.90f));
            Stretch(retryPanel.GetComponent<RectTransform>());
            LayoutElement retryLayout = retryPanel.AddComponent<LayoutElement>();
            retryLayout.ignoreLayout = true;
            retryPanel.transform.SetAsLastSibling();
            retryButton = CreateButton("RetryButton", retryPanel.transform, "RETRY", PrimaryButton, 30);
            SetCenteredSize(retryButton.GetComponent<RectTransform>(), new Vector2(320f, 86f));
            retryPanel.SetActive(false);
        }

        private static GameObject CreateStartingChoice(
            Transform parent,
            out Button stellaStartsButton,
            out Button krikStartsButton)
        {
            GameObject overlay = CreatePanel("StartingPlayerChoice", parent, new Color(0.01f, 0.03f, 0.06f, 0.92f));
            Stretch(overlay.GetComponent<RectTransform>());

            GameObject choiceCard = CreatePanel("ChoiceCard", overlay.transform, DialogueBackground);
            RectTransform cardRect = choiceCard.GetComponent<RectTransform>();
            SetCenteredSize(cardRect, new Vector2(900f, 300f));

            VerticalLayoutGroup cardLayout = choiceCard.AddComponent<VerticalLayoutGroup>();
            cardLayout.padding = new RectOffset(38, 38, 32, 32);
            cardLayout.spacing = 24f;
            cardLayout.childAlignment = TextAnchor.MiddleCenter;
            cardLayout.childControlWidth = true;
            cardLayout.childControlHeight = true;
            cardLayout.childForceExpandHeight = false;

            Text question = CreateText("Question", choiceCard.transform, "WHO BEGINS THE GREETING?", 34, TextAnchor.MiddleCenter);
            LayoutElement questionLayout = question.gameObject.AddComponent<LayoutElement>();
            questionLayout.preferredHeight = 76f;

            GameObject choices = new GameObject("Choices", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            choices.transform.SetParent(choiceCard.transform, false);
            LayoutElement choicesLayout = choices.GetComponent<LayoutElement>();
            choicesLayout.preferredHeight = 110f;
            choicesLayout.flexibleWidth = 1f;

            HorizontalLayoutGroup choicesGroup = choices.GetComponent<HorizontalLayoutGroup>();
            choicesGroup.spacing = 28f;
            choicesGroup.childAlignment = TextAnchor.MiddleCenter;
            choicesGroup.childControlWidth = true;
            choicesGroup.childControlHeight = true;
            choicesGroup.childForceExpandWidth = true;
            choicesGroup.childForceExpandHeight = true;

            stellaStartsButton = CreateButton("StellaStartsButton", choices.transform, "STELLA STARTS", new Color(0.18f, 0.52f, 0.86f, 1f), 28);
            krikStartsButton = CreateButton("KrikStartsButton", choices.transform, "KRIK STARTS", new Color(0.50f, 0.27f, 0.68f, 1f), 28);

            return overlay;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            string label,
            Color color,
            int fontSize)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            Image image = buttonObject.GetComponent<Image>();
            image.color = color;

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = color * 1.12f;
            colors.pressedColor = color * 0.82f;
            colors.disabledColor = new Color(0.18f, 0.23f, 0.29f, 0.72f);
            button.colors = colors;

            Text text = CreateText("Label", buttonObject.transform, label, fontSize, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform, 8f);
            text.raycastTarget = false;
            return button;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            string content,
            int fontSize,
            TextAnchor alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);

            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = LightText;
            text.supportRichText = false;
            return text;
        }

        private static void Stretch(RectTransform rectTransform, float inset = 0f)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(inset, inset);
            rectTransform.offsetMax = new Vector2(-inset, -inset);
        }

        private static void SetAnchors(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void SetCenteredSize(RectTransform rectTransform, Vector2 size)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = size;
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            string parentPath = Path.GetDirectoryName(assetPath).Replace('\\', '/');
            string folderName = Path.GetFileName(assetPath);
            EnsureFolder(parentPath);
            AssetDatabase.CreateFolder(parentPath, folderName);
        }

        private static void AddSceneToBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(SceneAssetPath, true)
            };
        }
    }
}
