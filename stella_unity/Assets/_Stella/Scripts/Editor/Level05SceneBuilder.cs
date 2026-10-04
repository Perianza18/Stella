using System.Collections.Generic;
using System.IO;
using System.Linq;
using Stella.Level05;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Stella.EditorTools
{
    /// <summary>Builds the small, reference-safe Level 5 placeholder scene.</summary>
    [InitializeOnLoad]
    public static class Level05SceneBuilder
    {
        public const string SceneAssetPath = "Assets/_Stella/Scenes/Level05_200Gems.unity";

        private static readonly Color PageBackground = new Color(0.035f, 0.05f, 0.06f, 1f);
        private static readonly Color PanelBackground = new Color(0.08f, 0.12f, 0.14f, 0.98f);
        private static readonly Color LightText = new Color(0.94f, 0.97f, 0.95f, 1f);

        static Level05SceneBuilder() { EditorApplication.delayCall += BuildSceneIfMissing; }

        [MenuItem("Stella/Build Level 5 Placeholder Scene")]
        public static void BuildScene()
        {
            EnsureFolder("Assets/_Stella");
            EnsureFolder("Assets/_Stella/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Level05_200Gems";

            GameObject root = new GameObject("Level05_200Gems");
            Level05Controller controller = root.AddComponent<Level05Controller>();
            CreateMainCamera(root.transform);
            CreateEventSystem(root.transform);
            Canvas canvas = CreateCanvas(root.transform);
            GameObject background = CreatePanel("Background", canvas.transform, PageBackground);
            Stretch(background.GetComponent<RectTransform>());
            background.transform.SetAsFirstSibling();

            CreateTopBar(canvas.transform);

            Text remainingText;
            Text rangeText;
            InputField amountField;
            Text historyText;
            Text noticeText;
            Button decreaseButton;
            Button increaseButton;
            Button confirmButton;
            GameObject retryPanel;
            Button retryButton;
            CreateMainRegion(
                canvas.transform,
                out remainingText,
                out rangeText,
                out amountField,
                out historyText,
                out noticeText,
                out decreaseButton,
                out increaseButton,
                out confirmButton,
                out retryPanel,
                out retryButton);

            controller.Configure(
                remainingText, rangeText, amountField, historyText, noticeText,
                decreaseButton, increaseButton, confirmButton, retryPanel, retryButton);

            EditorSceneManager.SaveScene(scene, SceneAssetPath);
            AddSceneToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created Stella Level 5 placeholder scene at " + SceneAssetPath);
        }

        public static void BuildSceneFromCommandLine() { BuildScene(); }

        private static void BuildSceneIfMissing()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneAssetPath) == null) BuildScene();
        }

        private static Canvas CreateCanvas(Transform parent)
        {
            GameObject canvasObject = new GameObject(
                "Level05Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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
            SetAnchors(top.GetComponent<RectTransform>(), new Vector2(0.035f, 0.885f), new Vector2(0.965f, 0.965f));
            HorizontalLayoutGroup layout = top.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 10, 10);
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
                "THE 200 GEMS\nTake the final gem before the Guardian. Each turn, take 1 up to half of what remains.",
                16, TextAnchor.MiddleLeft);
            rules.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        }

        private static void CreateMainRegion(
            Transform parent,
            out Text remainingText,
            out Text rangeText,
            out InputField amountField,
            out Text historyText,
            out Text noticeText,
            out Button decreaseButton,
            out Button increaseButton,
            out Button confirmButton,
            out GameObject retryPanel,
            out Button retryButton)
        {
            GameObject main = CreatePanel("MainRegion", parent, PanelBackground);
            SetAnchors(main.GetComponent<RectTransform>(), new Vector2(0.035f, 0.035f), new Vector2(0.965f, 0.825f));
            HorizontalLayoutGroup mainLayout = main.AddComponent<HorizontalLayoutGroup>();
            mainLayout.padding = new RectOffset(16, 16, 14, 14);
            mainLayout.spacing = 18f;
            mainLayout.childControlWidth = true;
            mainLayout.childControlHeight = true;
            mainLayout.childForceExpandWidth = false;
            mainLayout.childForceExpandHeight = true;

            // Left: the Guardian's record of every move and total it left behind.
            GameObject log = CreatePanel("HistoryRegion", main.transform, new Color(0.10f, 0.15f, 0.17f, 1f));
            log.AddComponent<LayoutElement>().flexibleWidth = 1f;
            VerticalLayoutGroup logLayout = log.AddComponent<VerticalLayoutGroup>();
            logLayout.padding = new RectOffset(18, 18, 14, 14);
            logLayout.spacing = 8f;
            logLayout.childControlWidth = true;
            logLayout.childControlHeight = true;
            logLayout.childForceExpandWidth = true;
            logLayout.childForceExpandHeight = false;

            CreateText("HistoryHeading", log.transform, "GUARDIAN'S RECORD", 19, TextAnchor.MiddleLeft)
                .gameObject.AddComponent<LayoutElement>().preferredHeight = 24f;

            GameObject scroll = CreatePanel("HistoryScroll", log.transform, new Color(0.07f, 0.11f, 0.12f, 1f));
            scroll.AddComponent<LayoutElement>().flexibleHeight = 1f;
            historyText = CreateText("HistoryText", scroll.transform, string.Empty, 16, TextAnchor.UpperLeft);
            historyText.verticalOverflow = VerticalWrapMode.Overflow;
            Stretch(historyText.rectTransform, 12f);

            // Right: how many gems to take, and the confirm/retry controls.
            GameObject controls = CreatePanel("ControlsRegion", main.transform, new Color(0.07f, 0.11f, 0.13f, 1f));
            LayoutElement controlsElement = controls.AddComponent<LayoutElement>();
            controlsElement.preferredWidth = 420f;
            controlsElement.minWidth = 360f;
            VerticalLayoutGroup controlsLayout = controls.AddComponent<VerticalLayoutGroup>();
            controlsLayout.padding = new RectOffset(16, 16, 14, 14);
            controlsLayout.spacing = 12f;
            controlsLayout.childAlignment = TextAnchor.UpperCenter;
            controlsLayout.childControlWidth = true;
            controlsLayout.childControlHeight = true;
            controlsLayout.childForceExpandWidth = true;
            controlsLayout.childForceExpandHeight = false;

            remainingText = CreateText("RemainingText", controls.transform, "200 GEMS LEFT", 24, TextAnchor.MiddleCenter);
            remainingText.gameObject.AddComponent<LayoutElement>().preferredHeight = 30f;

            rangeText = CreateText("RangeText", controls.transform, "TAKE 1-100", 19, TextAnchor.MiddleCenter);
            rangeText.gameObject.AddComponent<LayoutElement>().preferredHeight = 26f;

            GameObject stepper = CreatePanel("AmountStepper", controls.transform, new Color(0.10f, 0.15f, 0.17f, 1f));
            LayoutElement stepperElement = stepper.AddComponent<LayoutElement>();
            stepperElement.preferredHeight = 100f;
            HorizontalLayoutGroup stepperLayout = stepper.AddComponent<HorizontalLayoutGroup>();
            stepperLayout.padding = new RectOffset(14, 14, 10, 10);
            stepperLayout.spacing = 14f;
            stepperLayout.childAlignment = TextAnchor.MiddleCenter;
            stepperLayout.childControlWidth = true;
            stepperLayout.childControlHeight = true;
            stepperLayout.childForceExpandWidth = false;
            stepperLayout.childForceExpandHeight = false;

            decreaseButton = CreateButton("DecreaseButton", stepper.transform, "-", new Color(0.26f, 0.32f, 0.34f, 1f), 28);
            SetPreferredSize(decreaseButton.gameObject, new Vector2(80f, 80f));

            amountField = CreateNumberInputField("AmountField", stepper.transform, "1", 32);
            SetPreferredSize(amountField.gameObject, new Vector2(140f, 80f));

            increaseButton = CreateButton("IncreaseButton", stepper.transform, "+", new Color(0.26f, 0.32f, 0.34f, 1f), 28);
            SetPreferredSize(increaseButton.gameObject, new Vector2(80f, 80f));

            confirmButton = CreateButton("ConfirmButton", controls.transform, "TAKE GEMS", new Color(0.18f, 0.58f, 0.66f, 1f), 22);
            SetPreferredSize(confirmButton.gameObject, new Vector2(300f, 64f));

            noticeText = CreateText("NoticeText", controls.transform, string.Empty, 20, TextAnchor.MiddleCenter);
            noticeText.gameObject.AddComponent<LayoutElement>().preferredHeight = 50f;

            retryPanel = CreatePanel("RetryPanel", controls.transform, new Color(0.36f, 0.16f, 0.18f, 1f));
            SetPreferredSize(retryPanel, new Vector2(300f, 60f));
            retryButton = CreateButton("RetryButton", retryPanel.transform, "RETRY", new Color(0.73f, 0.25f, 0.25f, 1f), 18);
            Stretch(retryButton.GetComponent<RectTransform>(), 8f);
            retryPanel.SetActive(false);
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

        private static InputField CreateNumberInputField(string name, Transform parent, string content, int fontSize)
        {
            GameObject fieldObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
            fieldObject.transform.SetParent(parent, false);
            Image image = fieldObject.GetComponent<Image>();
            image.color = new Color(0.18f, 0.58f, 0.66f, 1f);
            image.raycastTarget = true;

            Text text = CreateText("Text", fieldObject.transform, content, fontSize, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform, 4f);
            text.raycastTarget = false;

            InputField field = fieldObject.GetComponent<InputField>();
            field.textComponent = text;
            field.contentType = InputField.ContentType.IntegerNumber;
            field.lineType = InputField.LineType.SingleLine;
            field.characterLimit = 3;
            field.text = content;
            ColorBlock colors = field.colors;
            colors.disabledColor = new Color(0.30f, 0.30f, 0.34f, 0.9f);
            field.colors = colors;
            return field;
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
