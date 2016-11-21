using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class Grid : Editor
{
    public static Vector3 currentHandlePosition = Vector3.zero;
    public static bool IsMouseInValidArea;
    private static Vector3 _mOldHandlePosition = Vector3.zero;

    public struct SizeGrid
    {
        public static float X
        {
            get { return EditorPrefs.GetFloat("SizeGridX", 0.5f); }
            set { EditorPrefs.SetFloat("SizeGridX", value); }
        }

        public static float Y
        {
            get { return EditorPrefs.GetFloat("SizeGridY", 0.5f); }
            set { EditorPrefs.SetFloat("SizeGridY", value); }
        }
    }

    public static int SelectedBlock {
        get { return EditorPrefs.GetInt("SelectedBlock", 0); }
        set { EditorPrefs.SetInt("SelectedBlock", value);}
    }

    private static int _selectedSizeGridX;
    private static int _selectedSizeGridY;
    private static int _selectedSizeGridParam;
    private static PrefabsObject _allPrefabs;

    //This is a public variable that gets or sets which of our custom tools we are currently using
    //0 - No tool selected
    //1 - The grid tool is selected
    public static bool IsInGridMode
    {
        get { return EditorPrefs.GetBool("IsInGridMode"); }
        set
        {
            if (value == IsInGridMode)
            {
                return;
            }
            EditorPrefs.SetBool("IsInGridMode", value);
            switch (value)
            {
                case false:
                    EditorPrefs.SetBool("IsLevelEditorEnabled", false);
                    Tools.hidden = false;
                    break;
                case true:
                    EditorPrefs.SetBool("IsLevelEditorEnabled", true);
                    EditorPrefs.SetFloat("CubeHandleColorR", Color.yellow.r);
                    EditorPrefs.SetFloat("CubeHandleColorG", Color.yellow.g);
                    EditorPrefs.SetFloat("CubeHandleColorB", Color.yellow.b);

                    //Hide Unitys Tool handles (like the move tool) while we draw our own stuff
                    Tools.hidden = true;
                    break;
                default:
                    EditorPrefs.SetBool("IsLevelEditorEnabled", true);
                    EditorPrefs.SetFloat("CubeHandleColorR", Color.yellow.r);
                    EditorPrefs.SetFloat("CubeHandleColorG", Color.yellow.g);
                    EditorPrefs.SetFloat("CubeHandleColorB", Color.yellow.b);

                    //Hide Unitys Tool handles (like the move tool) while we draw our own stuff
                    Tools.hidden = true;
                    break;
            }
        }
    }

    static Grid()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.onSceneGUIDelegate += OnSceneGUI;
        _allPrefabs = AssetDatabase.LoadAssetAtPath<PrefabsObject>( "Assets/GridConfig/Prefabs.asset" );
    }

    static void OnSceneGUI(SceneView _sceneView)
    {
        DrawToolsMenu(_sceneView.position);
        if (!IsInGridMode)
            return;
        SceneView.RepaintAll();
        UpdateHandlePosition();
        UpdateRepaint();
        DrawCubeDrawPreview();
        DrawCustomBlockButtons(_sceneView);
        DetectClick();
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
    }

    static void DrawToolsMenu(Rect _position)
    {
        //By using Handles.BeginGUI() we can start drawing regular GUI elements into the SceneView
        Handles.BeginGUI();

        //Here we draw a toolbar at the bottom edge of the SceneView
        GUILayout.BeginArea(new Rect(0, _position.height - 35, _position.width, 20), EditorStyles.toolbar);
        GUILayout.BeginHorizontal();
        IsInGridMode = GUILayout.Toggle(
            IsInGridMode,
            "Activate Grid",
            EditorStyles.toolbarButton);
        GUILayout.FlexibleSpace();
        if (IsInGridMode)
        {
            string[] _gridParams = {"X", "Y", "Both"};
            _selectedSizeGridParam = GUILayout.Toolbar(
                _selectedSizeGridParam,
                _gridParams,
                EditorStyles.toolbarButton);
            GUILayout.FlexibleSpace();
            string[] _allSizeGrid = {"1", "2", "4", "8", "16", "32"};
            switch (_selectedSizeGridParam)
            {
                case 0:
                    _selectedSizeGridX = GUILayout.Toolbar(
                        _selectedSizeGridX,
                        _allSizeGrid,
                        EditorStyles.toolbarButton);
                    SizeGrid.X = 0.5f * Mathf.Pow(2, _selectedSizeGridX);
                    break;
                case 1:
                    _selectedSizeGridY = GUILayout.Toolbar(
                        _selectedSizeGridY,
                        _allSizeGrid,
                        EditorStyles.toolbarButton);
                    SizeGrid.Y = 0.5f * Mathf.Pow(2, _selectedSizeGridY);
                    break;
                case 2:
                    _selectedSizeGridX = GUILayout.Toolbar(
                        _selectedSizeGridX,
                        _allSizeGrid,
                        EditorStyles.toolbarButton);
                    _selectedSizeGridY = _selectedSizeGridX;
                    SizeGrid.X = 0.5f * Mathf.Pow(2, _selectedSizeGridX);
                    SizeGrid.Y = 0.5f * Mathf.Pow(2, _selectedSizeGridX);
                    break;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        Handles.EndGUI();
    }

    static void UpdateHandlePosition()
    {
        if (Event.current == null)
        {
            return;
        }
        Vector2 _mousePosition = new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y);
        Vector3 _mousePosInWorld = HandleUtility.GUIPointToWorldRay(_mousePosition).origin;
        currentHandlePosition.x = Mathf.Round(_mousePosInWorld.x - SizeGrid.X) + SizeGrid.X;
        currentHandlePosition.y = Mathf.Round(_mousePosInWorld.y - SizeGrid.Y) + SizeGrid.Y;
        currentHandlePosition.z = 0;
    }

    static void UpdateRepaint()
    {
        //If the cube handle position has changed, repaint the scene
        if (currentHandlePosition == _mOldHandlePosition) return;
        SceneView.RepaintAll();
        _mOldHandlePosition = currentHandlePosition;
    }

    static void DrawCubeDrawPreview()
    {
        Handles.color = new Color(EditorPrefs.GetFloat("CubeHandleColorR", 1f), EditorPrefs.GetFloat("CubeHandleColorG", 1f), EditorPrefs.GetFloat("CubeHandleColorB", 0f));
        DrawHandlesCube(currentHandlePosition, SizeGrid.X, SizeGrid.Y);
    }

    static void DrawHandlesCube(Vector3 _center, float _offsetX, float _offsetY)
    {
        Vector3 _p1 = _center + Vector3.up * _offsetY + Vector3.right * _offsetX;
        Vector3 _p2 = _center + Vector3.up * _offsetY - Vector3.right * _offsetX;
        Vector3 _p3 = _center - Vector3.up * _offsetY - Vector3.right * _offsetX;
        Vector3 _p4 = _center - Vector3.up * _offsetY + Vector3.right * _offsetX;

        //You can use Handles to draw 3d objects into the SceneView. If defined properly the
        //user can even interact with the handles. For example Unitys move tool is implemented using Handles
        //However here we simply draw a cube that the 3D position the mouse is pointing to
        Handles.DrawLine(_p1, _p2);
        Handles.DrawLine(_p2, _p3);
        Handles.DrawLine(_p3, _p4);
        Handles.DrawLine(_p4, _p1);
    }

    void OnDestroy()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }

    static void DetectClick()
    {
        //If the left mouse is being clicked and no modifier buttons are being held
        if (Event.current.type == EventType.mouseDown &&
            Event.current.button == 0 &&
            Event.current.alt == false &&
            Event.current.shift == false &&
            Event.current.control == false)
        {
            AddBlock(currentHandlePosition, _allPrefabs.allPrefabs[SelectedBlock].prefab);
        }
        if (Event.current.type == EventType.mouseDown &&
            Event.current.button == 1 &&
            Event.current.alt == false &&
            Event.current.shift == false &&
            Event.current.control == false)
        {
            RemoveBlock(currentHandlePosition);
        }
    }

    static void RemoveBlock(Vector3 _currentHandlePosition)
    {
        RaycastHit _hit;
		Ray _newRay = new Ray();
		_newRay.origin = _currentHandlePosition - new Vector3 (0, 0, 50);
		_newRay.direction = new Vector3 (0, 0, 1);
		if (Physics.Raycast (_newRay, out _hit, 50)) 
		{
			if (_hit.transform.tag == "Environment")
				Undo.DestroyObjectImmediate (_hit.transform.gameObject);
		}
        EditorSceneManager.MarkAllScenesDirty();
    }

    static void AddBlock(Vector3 _currentHandlePosition, GameObject _prefab)
    {
        GameObject _instance = (GameObject) Instantiate(_prefab, _currentHandlePosition, Quaternion.identity);
        _instance.transform.localScale = new Vector3(SizeGrid.X, SizeGrid.Y, 0.5f) * 2;
        Undo.RegisterCreatedObjectUndo(_instance, "Create platform");
        EditorSceneManager.MarkAllScenesDirty();
    }

    //Draw a list of our custom blocks on the left side of the SceneView
    static void DrawCustomBlockButtons( SceneView _sceneView )
    {
        Handles.BeginGUI();

        GUI.Box( new Rect( 0, 0, 110, _sceneView.position.height - 35 ), GUIContent.none, EditorStyles.textArea );

        for( int i = 0; i < _allPrefabs.allPrefabs.Count; ++i )
        {
            DrawCustomBlockButton( i );
        }

        Handles.EndGUI();
    }

    static void DrawCustomBlockButton( int _index )
    {
        bool _isActive = _index == SelectedBlock;

        //By passing a Prefab or GameObject into AssetPreview.GetAssetPreview you get a texture that shows this object
        Texture2D _previewImage = AssetPreview.GetAssetPreview( _allPrefabs.allPrefabs[ _index ].prefab );
        GUIContent _buttonContent = new GUIContent( _previewImage );

        GUIStyle _text = GUIStyle.none;
        _text.normal.textColor = Color.black;
        GUI.Label( new Rect( 5, _index * 128 + 5, 100, 20 ), _allPrefabs.allPrefabs[ _index ].name, _text);
        bool _isToggleDown = GUI.Toggle( new Rect( 5, _index * 128 + 25, 100, 100 ), _isActive, _buttonContent, GUI.skin.button );

        //If this button is clicked but it wasn't clicked before (ie. if the user has just pressed the button)
        if( _isToggleDown && !_isActive)
            SelectedBlock = _index;
    }

}