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

            CreateTopBar(canvas.transform);

            Text lockOne;
            Text lockTwo;
            Text attemptText;
            Text noticeText;
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
            List<Image> attemptPips;
            CreateMainRegion(
                canvas.transform,
                controller,
                out lockOne,
                out lockTwo,
                out attemptText,
                out noticeText,
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
                out systemCard,
                out attemptPips);

            controller.Configure(
                lockOne, lockTwo, attemptText, noticeText, systemState, stellaState,
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

        private static void CreateTopBar(Transform parent)
        {
            GameObject top = CreatePanel("TopRegion", parent, PanelBackground);
            SetAnchors(top.GetComponent<RectTransform>(), new Vector2(0.035f, 0.895f), new Vector2(0.965f, 0.965f));
            HorizontalLayoutGroup layout = top.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 8, 8);
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            // A fixed corner reserved for Stella's expressive portrait (thinking, reacting, etc.).
            GameObject stellaFace = CreatePanel("StellaFace", top.transform, new Color(0.18f, 0.52f, 0.86f, 1f));
            SetPreferredSize(stellaFace, new Vector2(64f, 64f));
            Text stellaFaceLabel = CreateText("StellaFaceLabel", stellaFace.transform, "STELLA", 11, TextAnchor.MiddleCenter);
            Stretch(stellaFaceLabel.rectTransform, 4f);

            Text rules = CreateText(
                "RulesText", top.transform,
                "THE DOUBLE LOCK\nOpen both security locks by deducing each hidden 4-colour code before your attempts run out.",
                16, TextAnchor.MiddleLeft);
            rules.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        }

        private static void CreateMainRegion(
            Transform parent,
            Level01Controller controller,
            out Text lockOne,
            out Text lockTwo,
            out Text attemptText,
            out Text noticeText,
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
            out Image systemCard,
            out List<Image> attemptPips)
        {
            GameObject main = CreatePanel("MainRegion", parent, PanelBackground);
            SetAnchors(main.GetComponent<RectTransform>(), new Vector2(0.035f, 0.035f), new Vector2(0.965f, 0.875f));
            HorizontalLayoutGroup mainLayout = main.AddComponent<HorizontalLayoutGroup>();
            mainLayout.padding = new RectOffset(16, 16, 14, 14);
            mainLayout.spacing = 18f;
            mainLayout.childControlWidth = true;
            mainLayout.childControlHeight = true;
            mainLayout.childForceExpandWidth = false;
            mainLayout.childForceExpandHeight = true;

            // The attempt counter lives in the combination panel's lock-progress row and in
            // the numbered history rows below, so there is no separate attempts sidebar here.
            attemptPips = new List<Image>();

            // Left: the vault's log of previous attempts.
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

            // Right: the vault's own keypad, where the combination is actually entered.
            GameObject combination = CreatePanel("CombinationRegion", main.transform, new Color(0.07f, 0.11f, 0.13f, 1f));
            LayoutElement combinationElement = combination.AddComponent<LayoutElement>();
            // Wider than the other columns on purpose: this is where the vault door art
            // will eventually sit behind the code controls, so it needs the extra room.
            combinationElement.preferredWidth = 520f;
            combinationElement.minWidth = 440f;
            VerticalLayoutGroup combinationLayout = combination.AddComponent<VerticalLayoutGroup>();
            combinationLayout.padding = new RectOffset(16, 16, 10, 10);
            combinationLayout.spacing = 6f;
            combinationLayout.childAlignment = TextAnchor.UpperCenter;
            combinationLayout.childControlWidth = true;
            combinationLayout.childControlHeight = true;
            combinationLayout.childForceExpandWidth = true;
            combinationLayout.childForceExpandHeight = false;

            CreateText("CombinationHeading", combination.transform, "ENTER COMBINATION", 19, TextAnchor.MiddleCenter)
                .gameObject.AddComponent<LayoutElement>().preferredHeight = 24f;

            GameObject progression = CreatePanel("LockProgression", combination.transform, new Color(0.12f, 0.18f, 0.20f, 1f));
            LayoutElement progressionElement = progression.AddComponent<LayoutElement>();
            progressionElement.preferredHeight = 36f;
            HorizontalLayoutGroup progressLayout = progression.AddComponent<HorizontalLayoutGroup>();
            progressLayout.padding = new RectOffset(10, 10, 4, 4);
            progressLayout.spacing = 10f;
            progressLayout.childAlignment = TextAnchor.MiddleCenter;
            progressLayout.childControlWidth = true;
            progressLayout.childControlHeight = true;
            progressLayout.childForceExpandWidth = false;
            lockOne = CreateText("Lock1Label", progression.transform, "LOCK 1 ACTIVE", 16, TextAnchor.MiddleCenter);
            lockOne.gameObject.AddComponent<LayoutElement>().minWidth = 120f;
            CreateText("LockConnector", progression.transform, "—", 18, TextAnchor.MiddleCenter).gameObject.AddComponent<LayoutElement>().preferredWidth = 18f;
            lockTwo = CreateText("Lock2Label", progression.transform, "LOCK 2", 16, TextAnchor.MiddleCenter);
            lockTwo.gameObject.AddComponent<LayoutElement>().minWidth = 90f;
            attemptText = CreateText("AttemptText", progression.transform, "ATTEMPT 1/6", 16, TextAnchor.MiddleCenter);
            attemptText.gameObject.AddComponent<LayoutElement>().minWidth = 120f;

            noticeText = CreateText("NoticeText", combination.transform, string.Empty, 16, TextAnchor.MiddleCenter);
            noticeText.gameObject.AddComponent<LayoutElement>().preferredHeight = 36f;

            GameObject active = CreatePanel("ActiveGuessPanel", combination.transform, new Color(0.10f, 0.15f, 0.17f, 1f));
            LayoutElement activeElement = active.AddComponent<LayoutElement>();
            activeElement.preferredHeight = 80f;
            HorizontalLayoutGroup activeLayout = active.AddComponent<HorizontalLayoutGroup>();
            activeLayout.padding = new RectOffset(14, 14, 8, 8);
            activeLayout.spacing = 10f;
            activeLayout.childAlignment = TextAnchor.MiddleCenter;
            activeLayout.childControlWidth = true;
            activeLayout.childControlHeight = true;
            activeLayout.childForceExpandWidth = false;
            activeLayout.childForceExpandHeight = false;
            slotButtons = new List<Button>();
            slotSwatches = new List<Image>();
            for (int index = 0; index < DoubleLockGame.CodeLength; index++)
            {
                Button slot = CreateButton("GuessSlot_" + (index + 1), active.transform, string.Empty, new Color(0.16f, 0.21f, 0.23f, 1f), 18);
                SetPreferredSize(slot.gameObject, new Vector2(64f, 64f));
                slot.gameObject.AddComponent<Outline>().enabled = false;
                Image swatch = CreatePanel("Swatch", slot.transform, new Color(0.16f, 0.20f, 0.22f, 1f)).GetComponent<Image>();
                Stretch(swatch.rectTransform, 9f);
                slotButtons.Add(slot);
                slotSwatches.Add(swatch);
            }

            GameObject colors = CreatePanel("ColourControls", combination.transform, new Color(0.10f, 0.15f, 0.17f, 1f));
            LayoutElement colorsElement = colors.AddComponent<LayoutElement>();
            // 3 rows x 64 tall + 2 gaps x 8 + top/bottom padding of 10 each = 228. Give it a little slack.
            colorsElement.preferredHeight = 230f;
            GridLayoutGroup colorGrid = colors.AddComponent<GridLayoutGroup>();
            colorGrid.padding = new RectOffset(12, 12, 10, 10);
            colorGrid.spacing = new Vector2(8f, 8f);
            colorGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            colorGrid.constraintCount = 2;
            colorGrid.cellSize = new Vector2(140f, 64f);
            colorGrid.childAlignment = TextAnchor.UpperCenter;
            colorButtons = new List<Button>();
            string[] names = { "RED", "BLUE", "GREEN", "YELLOW", "PURPLE", "ORANGE" };
            for (int index = 0; index < names.Length; index++)
            {
                Button colorButton = CreateButton("ColourButton_" + names[index], colors.transform, names[index], Palette[index], 16);
                colorButtons.Add(colorButton);
            }

            GameObject actions = new GameObject("ActionRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            actions.transform.SetParent(combination.transform, false);
            actions.GetComponent<LayoutElement>().preferredHeight = 58f;
            HorizontalLayoutGroup actionLayout = actions.GetComponent<HorizontalLayoutGroup>();
            actionLayout.spacing = 12f;
            actionLayout.childAlignment = TextAnchor.MiddleCenter;
            actionLayout.childControlWidth = true;
            actionLayout.childControlHeight = true;
            actionLayout.childForceExpandWidth = false;
            actionLayout.childForceExpandHeight = false;
            clearButton = CreateButton("ClearButton", actions.transform, "CLEAR", new Color(0.26f, 0.32f, 0.34f, 1f), 19);
            SetPreferredSize(clearButton.gameObject, new Vector2(140f, 56f));
            submitButton = CreateButton("SubmitButton", actions.transform, "SUBMIT", new Color(0.18f, 0.58f, 0.66f, 1f), 20);
            SetPreferredSize(submitButton.gameObject, new Vector2(160f, 56f));

            Text statusHeading = CreateText("StatusHeading", combination.transform, "SECURITY STATE", 17, TextAnchor.MiddleCenter);
            SetPreferredSize(statusHeading.gameObject, new Vector2(220f, 22f));
            // Sized to actually hold Stella's (and the door's) expressive reaction art later on,
            // not just a status label.
            GameObject statusRow = new GameObject("StatusRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            statusRow.transform.SetParent(combination.transform, false);
            statusRow.GetComponent<LayoutElement>().preferredHeight = 150f;
            HorizontalLayoutGroup statusRowLayout = statusRow.GetComponent<HorizontalLayoutGroup>();
            statusRowLayout.spacing = 10f;
            statusRowLayout.childControlWidth = true;
            statusRowLayout.childControlHeight = true;
            statusRowLayout.childForceExpandWidth = true;
            stellaCard = CreatePanel("StellaCard", statusRow.transform, new Color(0.18f, 0.52f, 0.86f, 1f)).GetComponent<Image>();
            stellaState = CreateText("StellaState", stellaCard.transform, "STELLA\nREADY", 18, TextAnchor.MiddleCenter);
            Stretch(stellaState.rectTransform, 6f);
            systemCard = CreatePanel("SystemCard", statusRow.transform, new Color(0.24f, 0.53f, 0.62f, 1f)).GetComponent<Image>();
            systemState = CreateText("SystemState", systemCard.transform, "LOCK 1 ACTIVE", 16, TextAnchor.MiddleCenter);
            Stretch(systemState.rectTransform, 6f);

            retryPanel = CreatePanel("RetryPanel", combination.transform, new Color(0.36f, 0.16f, 0.18f, 1f));
            SetPreferredSize(retryPanel, new Vector2(300f, 60f));
            retryButton = CreateButton("RetryButton", retryPanel.transform, "RETRY FROM LOCK 1", new Color(0.73f, 0.25f, 0.25f, 1f), 17);
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
