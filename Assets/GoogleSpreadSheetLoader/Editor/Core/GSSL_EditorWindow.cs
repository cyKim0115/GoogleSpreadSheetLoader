using System;
using GoogleSpreadSheetLoader.Setting;
using UnityEditor;
using UnityEngine;

namespace GoogleSpreadSheetLoader
{
    public class GSSL_EditorWindow : EditorWindow
    {
        private readonly IntegratedView _integratedView = new();
        
        [MenuItem("Tools/Google Spread Sheet Loader")]
        public static void ShowWindow()
        {
            var window = GetWindow<GSSL_EditorWindow>(true, "Google Spread Sheet Loader");
            window.minSize = new Vector2(530, 600);
            window.ShowUtility();
        }

        private void OnEnable()
        {
            EditorApplication.update -= UpdateEditor;
            EditorApplication.update += UpdateEditor;
        }

        private void OnDisable()
        {
            EditorApplication.update -= UpdateEditor;
        }

        private DateTime _checkTime;
        
        private void UpdateEditor()
        {
            if((DateTime.Now - _checkTime).TotalSeconds > 1)
            {
                _checkTime = DateTime.Now;
                Repaint();
            }
        }
        
        private void OnGUI()
        {
            if (!GSSL_Setting.CheckAndCreate())
                return;
            
            _integratedView.DrawIntegratedView();
        }
    }
}