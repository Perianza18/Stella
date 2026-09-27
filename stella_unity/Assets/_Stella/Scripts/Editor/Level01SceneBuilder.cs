using System.Collections.Generic;
using System.IO;
using System.Linq;
using Stella.Level01;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Stella.EditorTools
{
    /// <summary>Builds the small, reference-safe Level 1 placeholder scene.</summary>
    [InitializeOnLoad]
    public static class Level01SceneBuilder
    {
        public const string SceneAssetPath = "Assets/_Stella/Scenes/Level01_DoubleLock.unity";

        private static readonly Color PageBackground = new Color(0.035f, 0.05f, 0.06f, 1f);
        private static readonly Color PanelBackground = new Color(0.08f, 0.12f, 0.14f, 0.98f);
        private static readonly Color ReasoningBackground = new Color(0.10f, 0.15f, 0.17f, 1f);
        private static readonly Color LightText = new Color(0.94f, 0.97f, 0.95f, 1f);
        private static readonly Color[] Palette =
        {
            new Color(0.88f, 0.20f, 0.22f, 1f), new Color(0.20f, 0.46f, 0.92f, 1f),
            new Color(0.20f, 0.72f, 0.35f, 1f), new Color(0.94f, 0.76f, 0.16f, 1f),
            new Color(0.64f, 0.30f, 0.82f, 1f), new Color(0.94f, 0.47f, 0.14f, 1f)
        };

        static Level01SceneBuilder() { EditorApplication.delayCall += BuildSceneIfMissing; }

        [MenuItem("Stella/Build Level 1 Placeholder Scene")]
        public static void BuildScene()
        {
            EnsureFolder("Assets/_Stella");
            EnsureFolder("Assets/_Stella/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Level01_DoubleLock";

            GameObject root = new GameObject("Level01_DoubleLock");
            Level01Controller controller = root.AddComponent<Level01Controller>();
            CreateMainCamera(root.transform);
            CreateEventSystem(root.transform);
            Canvas canvas = CreateCanvas(root.transform);
            GameObject background = CreatePanel("Background", canvas.transform, PageBackground);
            Stretch(background.GetComponent<RectTransform>());
            background.transform.SetAsFirstSibling();

            Text title;
            Text lockOne;
            Text lockTwo;
            Text attemptText;
            Text noticeText;
            List<Image> attemptPips;
            CreateTopBar(canvas.transform, out title, out lockOne, out lockTwo, out attemptText, out noticeText, out attemptPips);

            List<GuessHistoryRowView> historyRows;
            List<Button> slotButtons;
            List<Image> slotSwatches;
            List<Button> colorButtons;
            Button clearButton;
            Button submitButton;
            Text systemState;
            Text stellaState;
            Button retryButton;
            GameObject retryPanel;
            Image stellaCard;
            Image systemCard;
            CreateMainRegion(
                canvas.transform,
                controller,
                out historyRows,
                out slotButtons,
                out slotSwatches,
                out colorButtons,
                out clearButton,
                out submitButton,
                out systemState,
                out stellaState,
                out retryPanel,
                out retryButton,
                out stellaCard,
                out systemCard);

            controller.Configure(
                title, lockOne, lockTwo, attemptText, noticeText, systemState, stellaState,
                attemptPips, historyRows, slotButtons, slotSwatches, colorButtons,
                clearButton, submitButton, retryPanel, retryButton, stellaCard, systemCard);

            EditorSceneManager.SaveScene(scene, SceneAssetPath);
            AddSceneToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created Stella Level 1 placeholder scene at " + SceneAssetPath);
        }

        public static void BuildSceneFromCommandLine() { BuildScene(); }

        private static void BuildSceneIfMissing()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneAssetPath) == null) BuildScene();
        }

        private static Canvas CreateCanvas(Transform parent)
        {
            GameObject canvasObject = new GameObject(
                "Level01Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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

        private static void CreateTopBar(
            Transform parent,
            out Text title,
            out Text lockOne,
            out Text lockTwo,
            out Text attemptText,
            out Text noticeText,
            out List<Image> attemptPips)
        {
            GameObject top = CreatePanel("TopRegion", parent, PanelBackground);
            SetAnchors(top.GetComponent<RectTransform>(), new Vector2(0.035f, 0.825f), new Vector2(0.965f, 0.96f));
            HorizontalLayoutGroup layout = top.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 12, 12);
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            title = CreateText("LevelTitle", top.transform, "THE DOUBLE LOCK", 28, TextAnchor.MiddleLeft);
            title.gameObject.AddComponent<LayoutElement>().preferredWidth = 230f;
            GameObject progression = CreatePanel("LockProgression", top.transform, new Color(0.12f, 0.18f, 0.20f, 1f));
            progression.AddComponent<LayoutElement>().preferredWidth = 360f;
            HorizontalLayoutGroup progressLayout = progression.AddComponent<HorizontalLayoutGroup>();
            progressLayout.padding = new RectOffset(12, 12, 8, 8);
            progressLayout.spacing = 14f;
            progressLayout.childControlWidth = true;
            progressLayout.childControlHeight = true;
            progressLayout.childForceExpandWidth = true;
            lockOne = CreateText("Lock1Label", progression.transform, "LOCK 1 ACTIVE", 20, TextAnchor.MiddleCenter);
            CreateText("LockConnector", progression.transform, "—", 22, TextAnchor.MiddleCenter).gameObject.AddComponent<LayoutElement>().preferredWidth = 24f;
            lockTwo = CreateText("Lock2Label", progression.transform, "LOCK 2", 20, TextAnchor.MiddleCenter);
            attemptText = CreateText("AttemptText", top.transform, "LOCK 1 OF 2 — ATTEMPT 1/6", 21, TextAnchor.MiddleCenter);
            attemptText.gameObject.AddComponent<LayoutElement>().preferredWidth = 270f;
            GameObject attemptLights = CreatePanel("AttemptLights", top.transform, new Color(0.10f, 0.15f, 0.17f, 1f));
            attemptLights.AddComponent<LayoutElement>().preferredWidth = 220f;
            HorizontalLayoutGroup lightsLayout = attemptLights.AddComponent<HorizontalLayoutGroup>();
            lightsLayout.padding = new RectOffset(14, 14, 12, 12);
            lightsLayout.spacing = 8f;
            lightsLayout.childAlignment = TextAnchor.MiddleCenter;
            lightsLayout.childControlWidth = false;
            lightsLayout.childControlHeight = false;
            attemptPips = new List<Image>();
            for (int index = 0; index < DoubleLockGame.MaxAttempts; index++)
            {
                Image pip = CreatePanel("AttemptPip_" + (index + 1), attemptLights.transform, new Color(0.20f, 0.25f, 0.27f, 0.45f)).GetComponent<Image>();
                SetPreferredSize(pip.gameObject, new Vector2(22f, 22f));
                attemptPips.Add(pip);
            }
            noticeText = CreateText("NoticeText", top.transform, string.Empty, 20, TextAnchor.MiddleLeft);
            noticeText.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        }

        private static void CreateMainRegion(
            Transform parent,
            Level01Controller controller,
            out List<GuessHistoryRowView> historyRows,
            out List<Button> slotButtons,
            out List<Image> slotSwatches,
            out List<Button> colorButtons,
            out Button clearButton,
            out Button submitButton,
            out Text systemState,
            out Text stellaState,
            out GameObject retryPanel,
            out Button retryButton,
            out Image stellaCard,
            out Image systemCard)
        {
            GameObject main = CreatePanel("MainRegion", parent, PanelBackground);
            SetAnchors(main.GetComponent<RectTransform>(), new Vector2(0.035f, 0.035f), new Vector2(0.965f, 0.805f));
            HorizontalLayoutGroup mainLayout = main.AddComponent<HorizontalLayoutGroup>();
            mainLayout.padding = new RectOffset(16, 16, 14, 14);
            mainLayout.spacing = 18f;
            mainLayout.childControlWidth = true;
            mainLayout.childControlHeight = true;
            mainLayout.childForceExpandWidth = false;
            mainLayout.childForceExpandHeight = true;

            GameObject reasoning = CreatePanel("ReasoningRegion", main.transform, ReasoningBackground);
            reasoning.AddComponent<LayoutElement>().flexibleWidth = 1f;
            VerticalLayoutGroup reasoningLayout = reasoning.AddComponent<VerticalLayoutGroup>();
            reasoningLayout.padding = new RectOffset(18, 18, 12, 12);
            reasoningLayout.spacing = 8f;
            reasoningLayout.childControlWidth = true;
            reasoningLayout.childControlHeight = true;
            reasoningLayout.childForceExpandWidth = true;
            reasoningLayout.childForceExpandHeight = false;

            CreateText("HistoryHeading", reasoning.transform, "ATTEMPT HISTORY", 19, TextAnchor.MiddleLeft).gameObject.AddComponent<LayoutElement>().preferredHeight = 24f;
            GameObject history = new GameObject("HistoryRows", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            history.transform.SetParent(reasoning.transform, false);
            history.GetComponent<LayoutElement>().preferredHeight = 280f;
            VerticalLayoutGroup historyLayout = history.GetComponent<VerticalLayoutGroup>();
            historyLayout.spacing = 4f;
            historyLayout.childControlWidth = true;
            historyLayout.childControlHeight = true;
            historyLayout.childForceExpandWidth = true;
            historyLayout.childForceExpandHeight = false;
            historyRows = new List<GuessHistoryRowView>();
            for (int index = 0; index < DoubleLockGame.MaxAttempts; index++)
                historyRows.Add(CreateHistoryRow(history.transform, index + 1));

            CreateText("Legend", reasoning.transform, "Green = exact   Yellow = wrong position   Grey = no match", 15, TextAnchor.MiddleLeft).gameObject.AddComponent<LayoutElement>().preferredHeight = 20f;

            GameObject active = CreatePanel("ActiveGuessPanel", reasoning.transform, new Color(0.07f, 0.11f, 0.12f, 1f));
            LayoutElement activeElement = active.AddComponent<LayoutElement>();
            activeElement.preferredHeight = 88f;
            HorizontalLayoutGroup activeLayout = active.AddComponent<HorizontalLayoutGroup>();
            activeLayout.padding = new RectOffset(14, 14, 12, 12);
            activeLayout.spacing = 10f;
            activeLayout.childAlignment = TextAnchor.MiddleCenter;
            activeLayout.childControlWidth = false;
            activeLayout.childControlHeight = true;
            activeLayout.childForceExpandWidth = false;
            CreateText("ActiveGuessLabel", active.transform, "GUESS", 18, TextAnchor.MiddleCenter).gameObject.AddComponent<LayoutElement>().preferredWidth = 68f;
            slotButtons = new List<Button>();
            slotSwatches = new List<Image>();
            for (int index = 0; index < DoubleLockGame.CodeLength; index++)
            {
                Button slot = CreateButton("GuessSlot_" + (index + 1), active.transform, string.Empty, new Color(0.16f, 0.21f, 0.23f, 1f), 18);
                SetPreferredSize(slot.gameObject, new Vector2(70f, 70f));
                slot.gameObject.AddComponent<Outline>().enabled = false;
                Image swatch = CreatePanel("Swatch", slot.transform, new Color(0.16f, 0.20f, 0.22f, 1f)).GetComponent<Image>();
                Stretch(swatch.rectTransform, 9f);
                slotButtons.Add(slot);
                slotSwatches.Add(swatch);
            }

            GameObject colors = CreatePanel("ColourControls", reasoning.transform, new Color(0.07f, 0.11f, 0.12f, 1f));
            LayoutElement colorsElement = colors.AddComponent<LayoutElement>();
            colorsElement.preferredHeight = 76f;
            HorizontalLayoutGroup colorLayout = colors.AddComponent<HorizontalLayoutGroup>();
            colorLayout.padding = new RectOffset(12, 12, 10, 10);
            colorLayout.spacing = 8f;
            colorLayout.childControlWidth = true;
            colorLayout.childControlHeight = true;
            colorLayout.childForceExpandWidth = true;
            colorLayout.childForceExpandHeight = true;
            colorButtons = new List<Button>();
            string[] names = { "RED", "BLUE", "GREEN", "YELLOW", "PURPLE", "ORANGE" };
            for (int index = 0; index < names.Length; index++)
            {
                Button colorButton = CreateButton("ColourButton_" + names[index], colors.transform, names[index], Palette[index], 16);
                LayoutElement colorElement = colorButton.gameObject.AddComponent<LayoutElement>();
                colorElement.minWidth = 86f;
                colorElement.flexibleWidth = 1f;
                colorButtons.Add(colorButton);
            }

            GameObject actions = new GameObject("ActionRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            actions.transform.SetParent(reasoning.transform, false);
            actions.GetComponent<LayoutElement>().preferredHeight = 58f;
            HorizontalLayoutGroup actionLayout = actions.GetComponent<HorizontalLayoutGroup>();
            actionLayout.spacing = 12f;
            actionLayout.childAlignment = TextAnchor.MiddleCenter;
            actionLayout.childControlWidth = false;
            actionLayout.childControlHeight = true;
            actionLayout.childForceExpandWidth = false;
            clearButton = CreateButton("ClearButton", actions.transform, "CLEAR", new Color(0.26f, 0.32f, 0.34f, 1f), 19);
            SetPreferredSize(clearButton.gameObject, new Vector2(150f, 56f));
            submitButton = CreateButton("SubmitButton", actions.transform, "SUBMIT", new Color(0.18f, 0.58f, 0.66f, 1f), 20);
            SetPreferredSize(submitButton.gameObject, new Vector2(190f, 56f));

            GameObject status = CreatePanel("StatusRegion", main.transform, new Color(0.07f, 0.11f, 0.13f, 1f));
            LayoutElement statusElement = status.AddComponent<LayoutElement>();
            statusElement.preferredWidth = 285f;
            statusElement.minWidth = 250f;
            VerticalLayoutGroup statusLayout = status.AddComponent<VerticalLayoutGroup>();
            statusLayout.padding = new RectOffset(16, 16, 16, 16);
            statusLayout.spacing = 12f;
            statusLayout.childAlignment = TextAnchor.UpperCenter;
            statusLayout.childControlWidth = false;
            statusLayout.childControlHeight = true;
            statusLayout.childForceExpandWidth = false;
            statusLayout.childForceExpandHeight = false;
            Text statusHeading = CreateText("StatusHeading", status.transform, "SECURITY STATE", 19, TextAnchor.MiddleCenter);
            SetPreferredSize(statusHeading.gameObject, new Vector2(220f, 28f));
            stellaCard = CreatePanel("StellaCard", status.transform, new Color(0.18f, 0.52f, 0.86f, 1f)).GetComponent<Image>();
            SetPreferredSize(stellaCard.gameObject, new Vector2(190f, 150f));
            stellaState = CreateText("StellaState", stellaCard.transform, "STELLA\nREADY", 22, TextAnchor.MiddleCenter);
            Stretch(stellaState.rectTransform, 8f);
            systemCard = CreatePanel("SystemCard", status.transform, new Color(0.24f, 0.53f, 0.62f, 1f)).GetComponent<Image>();
            SetPreferredSize(systemCard.gameObject, new Vector2(190f, 150f));
            systemState = CreateText("SystemState", systemCard.transform, "LOCK 1 ACTIVE", 19, TextAnchor.MiddleCenter);
            Stretch(systemState.rectTransform, 8f);
            retryPanel = CreatePanel("RetryPanel", status.transform, new Color(0.36f, 0.16f, 0.18f, 1f));
            SetPreferredSize(retryPanel, new Vector2(220f, 68f));
            retryButton = CreateButton("RetryButton", retryPanel.transform, "RETRY FROM LOCK 1", new Color(0.73f, 0.25f, 0.25f, 1f), 18);
            Stretch(retryButton.GetComponent<RectTransform>(), 8f);
            retryPanel.SetActive(false);
        }

        private static GuessHistoryRowView CreateHistoryRow(Transform parent, int number)
        {
            GameObject row = CreatePanel("HistoryRow_" + number, parent, new Color(0.12f, 0.17f, 0.19f, 0.85f));
            LayoutElement rowElement = row.AddComponent<LayoutElement>();
            rowElement.preferredHeight = 40f;
            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 4, 4);
            layout.spacing = 7f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            Text attempt = CreateText("AttemptNumber", row.transform, "—", 17, TextAnchor.MiddleCenter);
            SetPreferredSize(attempt.gameObject, new Vector2(30f, 30f));
            List<Image> guesses = new List<Image>();
            for (int index = 0; index < DoubleLockGame.CodeLength; index++)
            {
                Image swatch = CreatePanel("GuessColour_" + (index + 1), row.transform, new Color(0.16f, 0.20f, 0.22f, 0.45f)).GetComponent<Image>();
                SetPreferredSize(swatch.gameObject, new Vector2(30f, 30f));
                guesses.Add(swatch);
            }
            CreateText("FeedbackDivider", row.transform, "·", 18, TextAnchor.MiddleCenter).gameObject.AddComponent<LayoutElement>().preferredWidth = 14f;
            GameObject feedbackGroup = new GameObject("FeedbackGroup", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
            feedbackGroup.transform.SetParent(row.transform, false);
            SetPreferredSize(feedbackGroup, new Vector2(40f, 34f));
            GridLayoutGroup feedbackGrid = feedbackGroup.GetComponent<GridLayoutGroup>();
            feedbackGrid.cellSize = new Vector2(15f, 15f);
            feedbackGrid.spacing = new Vector2(4f, 4f);
            feedbackGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            feedbackGrid.constraintCount = 2;
            List<Image> feedback = new List<Image>();
            for (int index = 0; index < DoubleLockGame.CodeLength; index++)
            {
                Image pip = CreatePanel("FeedbackPip_" + (index + 1), feedbackGroup.transform, new Color(0.12f, 0.15f, 0.16f, 0.25f)).GetComponent<Image>();
                feedback.Add(pip);
            }
            GuessHistoryRowView view = row.AddComponent<GuessHistoryRowView>();
            view.Configure(attempt, guesses.ToArray(), feedback.ToArray());
            return view;
        }

        private static Camera CreateMainCamera(Transform parent)
        {
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.localPosition = new Vector3(0f, 0f, -10f);
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = PageBackground;
            return camera;
        }

        private static void CreateEventSystem(Transform parent)
        {
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystem.transform.SetParent(parent, false);
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            Image image = panel.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return panel;
        }

        private static Button CreateButton(string name, Transform parent, string label, Color color, int fontSize)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = color * 1.12f;
            colors.pressedColor = color * 0.82f;
            colors.disabledColor = new Color(0.18f, 0.22f, 0.24f, 0.7f);
            button.colors = colors;
            Text text = CreateText("Label", buttonObject.transform, label, fontSize, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform, 6f);
            text.raycastTarget = false;
            return button;
        }

        private static Text CreateText(string name, Transform parent, string content, int fontSize, TextAnchor alignment)
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
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static void SetPreferredSize(GameObject gameObject, Vector2 size)
        {
            LayoutElement element = gameObject.GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = size.x;
            element.preferredHeight = size.y;
        }

        private static void SetPreferredSize(Image image, Vector2 size) { SetPreferredSize(image.gameObject, size); }

        private static void Stretch(RectTransform rectTransform, float inset = 0f)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(inset, inset);
            rectTransform.offsetMax = new Vector2(-inset, -inset);
        }

        private static void SetAnchors(RectTransform rectTransform, Vector2 minimum, Vector2 maximum)
        {
            rectTransform.anchorMin = minimum;
            rectTransform.anchorMax = maximum;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;
            string parent = Path.GetDirectoryName(assetPath).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(assetPath));
        }

        private static void AddSceneToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.All(scene => scene.path != SceneAssetPath))
                scenes.Add(new EditorBuildSettingsScene(SceneAssetPath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
