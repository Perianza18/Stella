using System.Collections.Generic;
using System.IO;
using System.Linq;
using Stella.Level02;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Stella.EditorTools
{
    /// <summary>Builds the replaceable, reference-safe Level 2 placeholder scene.</summary>
    [InitializeOnLoad]
    public static class Level02SceneBuilder
    {
        public const string SceneAssetPath = "Assets/_Stella/Scenes/Level02_RuinsBoard.unity";

        private static readonly Color PageBackground = new Color(0.035f, 0.055f, 0.06f, 1f);
        private static readonly Color PanelBackground = new Color(0.09f, 0.13f, 0.14f, 0.98f);
        private static readonly Color DialogueBackground = new Color(0.12f, 0.18f, 0.19f, 1f);
        private static readonly Color PrimaryButton = new Color(0.18f, 0.58f, 0.66f, 1f);
        private static readonly Color LightText = new Color(0.94f, 0.97f, 0.94f, 1f);

        static Level02SceneBuilder() { EditorApplication.delayCall += BuildSceneIfMissing; }

        [MenuItem("Stella/Build Level 2 Placeholder Scene")]
        public static void BuildScene()
        {
            EnsureFolder("Assets/_Stella");
            EnsureFolder("Assets/_Stella/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Level02_RuinsBoard";

            GameObject root = new GameObject("Level02_RuinsBoard");
            Level02Controller controller = root.AddComponent<Level02Controller>();
            CreateMainCamera(root.transform);
            CreateEventSystem(root.transform);

            Canvas canvas = CreateCanvas(root.transform);
            GameObject background = CreatePanel("Background", canvas.transform, PageBackground);
            Stretch(background.GetComponent<RectTransform>());
            background.transform.SetAsFirstSibling();

            List<BoardCellView> cells;
            Text activeCardLabel;
            Text dialogueSpeakerText;
            Text dialogueBodyText;
            Button contextActionButton;
            CreateGameplayRegion(
                canvas.transform,
                controller,
                out cells,
                out activeCardLabel,
                out dialogueSpeakerText,
                out dialogueBodyText,
                out contextActionButton);

            Button stellaStartsButton;
            Button keeperStartsButton;
            GameObject startingChoicePanel = CreateStartingChoice(
                canvas.transform,
                out stellaStartsButton,
                out keeperStartsButton);

            controller.Configure(
                cells,
                activeCardLabel,
                dialogueSpeakerText,
                dialogueBodyText,
                contextActionButton,
                startingChoicePanel,
                stellaStartsButton,
                keeperStartsButton);

            EditorSceneManager.SaveScene(scene, SceneAssetPath);
            AddSceneToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created Stella Level 2 placeholder scene at " + SceneAssetPath);
        }

        public static void BuildSceneFromCommandLine() { BuildScene(); }

        private static void BuildSceneIfMissing()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneAssetPath) == null) BuildScene();
        }

        private static Canvas CreateCanvas(Transform parent)
        {
            GameObject canvasObject = new GameObject(
                "Level02Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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

        private static void CreateGameplayRegion(
            Transform parent,
            Level02Controller controller,
            out List<BoardCellView> cells,
            out Text activeCardLabel,
            out Text dialogueSpeakerText,
            out Text dialogueBodyText,
            out Button contextActionButton)
        {
            GameObject gameplay = CreatePanel("GameplayRegion", parent, PanelBackground);
            SetAnchors(gameplay.GetComponent<RectTransform>(), new Vector2(0.025f, 0.025f), new Vector2(0.975f, 0.975f));
            HorizontalLayoutGroup layout = gameplay.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 14, 14);
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            GameObject narrative = CreatePanel("NarrativePanel", gameplay.transform, DialogueBackground);
            LayoutElement narrativeLayout = narrative.AddComponent<LayoutElement>();
            narrativeLayout.preferredWidth = 365f;
            narrativeLayout.minWidth = 340f;
            narrativeLayout.flexibleWidth = 0f;
            VerticalLayoutGroup narrativeColumn = narrative.AddComponent<VerticalLayoutGroup>();
            narrativeColumn.padding = new RectOffset(24, 24, 22, 22);
            narrativeColumn.spacing = 16f;
            narrativeColumn.childControlWidth = true;
            narrativeColumn.childControlHeight = true;
            narrativeColumn.childForceExpandWidth = true;
            narrativeColumn.childForceExpandHeight = false;

            GameObject battleCardSlot = new GameObject("BattleCardSlot", typeof(RectTransform), typeof(LayoutElement));
            battleCardSlot.transform.SetParent(narrative.transform, false);
            battleCardSlot.GetComponent<LayoutElement>().preferredHeight = 220f;
            GameObject battleCard = CreatePanel("BattleCard", battleCardSlot.transform, new Color(0.34f, 0.40f, 0.32f, 1f));
            SetCenteredSize(battleCard.GetComponent<RectTransform>(), new Vector2(220f, 220f));
            activeCardLabel = CreateText("ActiveCardLabel", battleCard.transform, "KEEPER", 31, TextAnchor.MiddleCenter);
            Stretch(activeCardLabel.rectTransform, 12f);

            GameObject dialogueBox = CreatePanel("DialogueBox", narrative.transform, new Color(0.075f, 0.115f, 0.12f, 1f));
            LayoutElement dialogueLayout = dialogueBox.AddComponent<LayoutElement>();
            dialogueLayout.flexibleHeight = 1f;
            VerticalLayoutGroup dialogueColumn = dialogueBox.AddComponent<VerticalLayoutGroup>();
            dialogueColumn.padding = new RectOffset(22, 22, 20, 20);
            dialogueColumn.spacing = 12f;
            dialogueColumn.childControlWidth = true;
            dialogueColumn.childControlHeight = true;
            dialogueColumn.childForceExpandHeight = false;
            dialogueSpeakerText = CreateText("DialogueSpeakerText", dialogueBox.transform, "KEEPER", 20, TextAnchor.MiddleLeft);
            dialogueSpeakerText.color = new Color(0.72f, 0.82f, 0.68f, 1f);
            SetPreferredHeight(dialogueSpeakerText.gameObject, 34f);
            dialogueBodyText = CreateText("DialogueBodyText", dialogueBox.transform, "Who will place the first stone?", 27, TextAnchor.UpperLeft);
            dialogueBodyText.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            dialogueBodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            dialogueBodyText.verticalOverflow = VerticalWrapMode.Truncate;

            GameObject actionArea = new GameObject("ActionArea", typeof(RectTransform), typeof(LayoutElement));
            actionArea.transform.SetParent(narrative.transform, false);
            actionArea.GetComponent<LayoutElement>().preferredHeight = 116f;

            contextActionButton = CreateButton("ContextActionButton", actionArea.transform, "PLACE PIECE", PrimaryButton, 27);
            SetAnchors(contextActionButton.GetComponent<RectTransform>(), new Vector2(0f, 0.12f), new Vector2(1f, 0.88f));
            contextActionButton.interactable = false;

            GameObject boardRegion = CreatePanel("BoardRegion", gameplay.transform, new Color(0.055f, 0.08f, 0.08f, 1f));
            LayoutElement boardRegionLayout = boardRegion.AddComponent<LayoutElement>();
            boardRegionLayout.flexibleWidth = 1f;
            boardRegionLayout.minWidth = 900f;

            GameObject gridObject = new GameObject("BoardGrid_7x7", typeof(RectTransform), typeof(GridLayoutGroup));
            gridObject.transform.SetParent(boardRegion.transform, false);
            SetCenteredSize(gridObject.GetComponent<RectTransform>(), new Vector2(776f, 776f));
            GridLayoutGroup grid = gridObject.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(104f, 104f);
            grid.spacing = new Vector2(8f, 8f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = RuinsBoardGame.BoardSize;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;

            cells = new List<BoardCellView>();
            for (int row = 0; row < RuinsBoardGame.BoardSize; row++)
            {
                for (int column = 0; column < RuinsBoardGame.BoardSize; column++)
                {
                    Button button = CreateButton("Cell_" + row + "_" + column, gridObject.transform, string.Empty,
                        new Color(0.38f, 0.42f, 0.43f, 1f), 24);
                    BoardCellView view = button.gameObject.AddComponent<BoardCellView>();
                    view.Configure(row, column, button.GetComponent<Image>(), button,
                        button.GetComponentInChildren<Text>(), controller.HandleCellPressed);
                    cells.Add(view);
                }
            }
        }

        private static GameObject CreateStartingChoice(
            Transform parent,
            out Button stellaStartsButton,
            out Button keeperStartsButton)
        {
            GameObject overlay = CreatePanel("StartingPlayerChoice", parent, new Color(0.01f, 0.025f, 0.025f, 0.92f));
            Stretch(overlay.GetComponent<RectTransform>());

            GameObject card = CreatePanel("StartingChoiceCard", overlay.transform, DialogueBackground);
            SetCenteredSize(card.GetComponent<RectTransform>(), new Vector2(940f, 320f));
            VerticalLayoutGroup layout = card.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(40, 40, 34, 34);
            layout.spacing = 25f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            Text question = CreateText(
                "StartingQuestion",
                card.transform,
                "WHO SHOULD PLACE THE FIRST STONE?",
                34,
                TextAnchor.MiddleCenter);
            SetPreferredHeight(question.gameObject, 78f);

            GameObject choices = new GameObject(
                "StartingChoices", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            choices.transform.SetParent(card.transform, false);
            choices.GetComponent<LayoutElement>().preferredHeight = 112f;
            HorizontalLayoutGroup choiceLayout = choices.GetComponent<HorizontalLayoutGroup>();
            choiceLayout.spacing = 28f;
            choiceLayout.childControlWidth = true;
            choiceLayout.childControlHeight = true;
            choiceLayout.childForceExpandWidth = true;
            choiceLayout.childForceExpandHeight = true;

            stellaStartsButton = CreateButton(
                "StellaStartsButton", choices.transform, "STELLA STARTS", new Color(0.16f, 0.48f, 0.88f, 1f), 27);
            keeperStartsButton = CreateButton(
                "KeeperStartsButton", choices.transform, "KEEPER STARTS", new Color(0.46f, 0.22f, 0.68f, 1f), 27);
            return overlay;
        }

        private static Camera CreateMainCamera(Transform parent)
        {
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(parent, false);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.localPosition = new Vector3(0f, 0f, -10f);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = PageBackground;
            camera.depth = -1f;
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
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static Button CreateButton(string name, Transform parent, string label, Color color, int fontSize)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = color * 1.12f;
            colors.pressedColor = color * 0.82f;
            colors.disabledColor = color;
            colors.colorMultiplier = 1f;
            button.colors = colors;
            Text text = CreateText("Label", buttonObject.transform, label, fontSize, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform, 7f);
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
            return text;
        }

        private static void SetPreferredHeight(GameObject gameObject, float height)
        {
            LayoutElement element = gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = height;
        }

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
