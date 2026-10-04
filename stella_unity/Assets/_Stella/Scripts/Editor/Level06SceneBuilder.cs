using System.Collections.Generic;
using System.IO;
using System.Linq;
using Stella.Level06;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Stella.EditorTools
{
    /// <summary>Builds the small, reference-safe Level 6 placeholder scene.</summary>
    [InitializeOnLoad]
    public static class Level06SceneBuilder
    {
        public const string SceneAssetPath = "Assets/_Stella/Scenes/Level06_GalacticCustoms.unity";

        private static readonly Color PageBackground = new Color(0.03f, 0.04f, 0.07f, 1f);
        private static readonly Color PanelBackground = new Color(0.08f, 0.10f, 0.16f, 0.98f);
        private static readonly Color LightText = new Color(0.93f, 0.95f, 0.99f, 1f);
        private static readonly Color AccentBlue = new Color(0.24f, 0.47f, 0.86f, 1f);
        private static readonly Color AccentPurple = new Color(0.46f, 0.30f, 0.80f, 1f);

        static Level06SceneBuilder() { EditorApplication.delayCall += BuildSceneIfMissing; }

        [MenuItem("Stella/Build Level 6 Placeholder Scene")]
        public static void BuildScene()
        {
            EnsureFolder("Assets/_Stella");
            EnsureFolder("Assets/_Stella/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Level06_GalacticCustoms";

            GameObject root = new GameObject("Level06_GalacticCustoms");
            Level06Controller controller = root.AddComponent<Level06Controller>();
            CreateMainCamera(root.transform);
            CreateEventSystem(root.transform);
            Canvas canvas = CreateCanvas(root.transform);
            GameObject background = CreatePanel("Background", canvas.transform, PageBackground);
            Stretch(background.GetComponent<RectTransform>());
            background.transform.SetAsFirstSibling();

            CreateTopBar(canvas.transform);

            Text usesText;
            Text candidateCountText;
            Text noticeText;
            List<Button> eggButtons;
            List<Image> eggSwatches;
            Button weighButton;
            Button guessButton;
            GameObject retryPanel;
            Button retryButton;
            CreateMainRegion(
                canvas.transform,
                out usesText,
                out candidateCountText,
                out noticeText,
                out eggButtons,
                out eggSwatches,
                out weighButton,
                out guessButton,
                out retryPanel,
                out retryButton);

            controller.Configure(
                usesText, candidateCountText, noticeText,
                eggButtons, eggSwatches, weighButton, guessButton, retryPanel, retryButton);

            EditorSceneManager.SaveScene(scene, SceneAssetPath);
            AddSceneToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created Stella Level 6 placeholder scene at " + SceneAssetPath);
        }

        public static void BuildSceneFromCommandLine() { BuildScene(); }

        private static void BuildSceneIfMissing()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneAssetPath) == null) BuildScene();
        }

        private static Canvas CreateCanvas(Transform parent)
        {
            GameObject canvasObject = new GameObject(
                "Level06Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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
            GameObject stellaFace = CreatePanel("StellaFace", top.transform, AccentBlue);
            SetPreferredSize(stellaFace, new Vector2(64f, 64f));
            Text stellaFaceLabel = CreateText("StellaFaceLabel", stellaFace.transform, "STELLA", 11, TextAnchor.MiddleCenter);
            Stretch(stellaFaceLabel.rectTransform, 4f);

            Text rules = CreateText(
                "RulesText", top.transform,
                "GALACTIC CUSTOMS\nFind the heavier egg among 27 using the scale at most 3 times, then name the stowaway.",
                16, TextAnchor.MiddleLeft);
            rules.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        }

        private static void CreateMainRegion(
            Transform parent,
            out Text usesText,
            out Text candidateCountText,
            out Text noticeText,
            out List<Button> eggButtons,
            out List<Image> eggSwatches,
            out Button weighButton,
            out Button guessButton,
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

            // Left: the alien balance. A placeholder panel with weigh/guess actions and the result.
            GameObject scale = CreatePanel("BalanceRegion", main.transform, new Color(0.07f, 0.09f, 0.14f, 1f));
            LayoutElement scaleElement = scale.AddComponent<LayoutElement>();
            scaleElement.preferredWidth = 380f;
            scaleElement.minWidth = 320f;
            VerticalLayoutGroup scaleLayout = scale.AddComponent<VerticalLayoutGroup>();
            scaleLayout.padding = new RectOffset(16, 16, 14, 14);
            scaleLayout.spacing = 12f;
            scaleLayout.childAlignment = TextAnchor.UpperCenter;
            scaleLayout.childControlWidth = true;
            scaleLayout.childControlHeight = true;
            scaleLayout.childForceExpandWidth = true;
            scaleLayout.childForceExpandHeight = false;

            GameObject statusRow = new GameObject("StatusRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            statusRow.transform.SetParent(scale.transform, false);
            statusRow.GetComponent<LayoutElement>().preferredHeight = 28f;
            HorizontalLayoutGroup statusLayout = statusRow.GetComponent<HorizontalLayoutGroup>();
            statusLayout.spacing = 10f;
            statusLayout.childControlWidth = true;
            statusLayout.childControlHeight = true;
            statusLayout.childForceExpandWidth = true;
            usesText = CreateText("UsesText", statusRow.transform, "BATTERY: 3/3", 16, TextAnchor.MiddleCenter);
            candidateCountText = CreateText("CandidateCountText", statusRow.transform, "27 SUSPECT(S) LEFT", 16, TextAnchor.MiddleCenter);

            CreateText("BalanceHeading", scale.transform, "ALIEN BALANCE", 19, TextAnchor.MiddleCenter)
                .gameObject.AddComponent<LayoutElement>().preferredHeight = 24f;

            GameObject helpText = CreateText(
                "InstructionText", scale.transform,
                "Tap suspects to cycle them: unassigned -> left pan -> right pan -> unassigned.",
                15, TextAnchor.MiddleCenter).gameObject;
            helpText.AddComponent<LayoutElement>().preferredHeight = 60f;
            helpText.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Wrap;

            weighButton = CreateButton("WeighButton", scale.transform, "WEIGH", AccentBlue, 22);
            SetPreferredSize(weighButton.gameObject, new Vector2(300f, 64f));

            guessButton = CreateButton("GuessButton", scale.transform, "CONFIRM GUESS", AccentPurple, 20);
            SetPreferredSize(guessButton.gameObject, new Vector2(300f, 64f));

            noticeText = CreateText("NoticeText", scale.transform, string.Empty, 18, TextAnchor.MiddleCenter);
            noticeText.horizontalOverflow = HorizontalWrapMode.Wrap;
            noticeText.gameObject.AddComponent<LayoutElement>().preferredHeight = 70f;

            retryPanel = CreatePanel("RetryPanel", scale.transform, new Color(0.36f, 0.16f, 0.18f, 1f));
            SetPreferredSize(retryPanel, new Vector2(300f, 60f));
            retryButton = CreateButton("RetryButton", retryPanel.transform, "RETRY", new Color(0.73f, 0.25f, 0.25f, 1f), 18);
            Stretch(retryButton.GetComponent<RectTransform>(), 8f);
            retryPanel.SetActive(false);

            // Right: the 27 candidate eggs.
            GameObject eggsRegion = CreatePanel("EggsRegion", main.transform, new Color(0.07f, 0.09f, 0.14f, 1f));
            eggsRegion.AddComponent<LayoutElement>().flexibleWidth = 1f;
            VerticalLayoutGroup eggsLayout = eggsRegion.AddComponent<VerticalLayoutGroup>();
            eggsLayout.padding = new RectOffset(16, 16, 14, 14);
            eggsLayout.spacing = 10f;
            eggsLayout.childControlWidth = true;
            eggsLayout.childControlHeight = true;
            eggsLayout.childForceExpandWidth = true;
            eggsLayout.childForceExpandHeight = false;

            CreateText("EggsHeading", eggsRegion.transform, "27 METEOR EGGS", 19, TextAnchor.MiddleLeft)
                .gameObject.AddComponent<LayoutElement>().preferredHeight = 24f;

            GameObject grid = new GameObject("EggGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
            grid.transform.SetParent(eggsRegion.transform, false);
            grid.GetComponent<LayoutElement>().flexibleHeight = 1f;
            GridLayoutGroup gridLayout = grid.GetComponent<GridLayoutGroup>();
            gridLayout.padding = new RectOffset(4, 4, 4, 4);
            gridLayout.spacing = new Vector2(8f, 8f);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 9;
            gridLayout.cellSize = new Vector2(84f, 84f);
            gridLayout.childAlignment = TextAnchor.UpperCenter;

            eggButtons = new List<Button>();
            eggSwatches = new List<Image>();
            for (int eggNumber = 1; eggNumber <= GalacticCustomsGame.TotalEggs; eggNumber++)
            {
                Button eggButton = CreateButton(
                    "Egg_" + eggNumber, grid.transform, eggNumber.ToString(), new Color(0.16f, 0.21f, 0.23f, 1f), 18);
                eggButtons.Add(eggButton);
                eggSwatches.Add(eggButton.GetComponent<Image>());
            }
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
            colors.disabledColor = new Color(0.14f, 0.15f, 0.17f, 0.6f);
            button.colors = colors;
            Text text = CreateText("Label", buttonObject.transform, label, fontSize, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform, 4f);
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
