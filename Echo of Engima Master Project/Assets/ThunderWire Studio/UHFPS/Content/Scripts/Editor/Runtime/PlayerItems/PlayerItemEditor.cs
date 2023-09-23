using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using UHFPS.Scriptable;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    public class PlayerItemEditor<T> : Editor where T : PlayerItemBehaviour
    {
        public T Target { get; private set; }
        public PlayerItemBehaviour Behaviour { get; private set; }
        public PropertyCollection Properties { get; private set; }

        private bool settingsFoldout;
        private MotionListHelper motionListHelper;

        public virtual void OnEnable()
        {
            Target = target as T;
            Behaviour = Target;
            Properties = EditorDrawing.GetAllProperties(serializedObject);

            MotionPreset preset = Behaviour.MotionPreset;
            motionListHelper = new(preset);
        }

        public override void OnInspectorGUI()
        {
            GUIContent playerItemSettingsContent = EditorGUIUtility.TrTextContentWithIcon(" PlayerItem Base Settings", "Settings");
            if (EditorDrawing.BeginFoldoutBorderLayout(playerItemSettingsContent, ref settingsFoldout))
            {
                if(EditorDrawing.BeginFoldoutToggleBorderLayout(new GUIContent("Wall Detection"), Properties["EnableWallDetection"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("EnableWallDetection")))
                    {
                        Properties.Draw("ShowRayGizmos");
                        Properties.Draw("WallHitMask");
                        Properties.Draw("WallHitRayDistance");
                        Properties.Draw("WallHitRayRadius");
                        Properties.Draw("WallHitAmount");
                        Properties.Draw("WallHitTime");
                        Properties.Draw("WallHitRayOffset");
                    }
                    EditorDrawing.EndBorderHeaderLayout();
                }

                if (EditorDrawing.BeginFoldoutToggleBorderLayout(new GUIContent("Motion Preset"), Properties["EnableMotionPreset"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("EnableMotionPreset")))
                    {
                        motionListHelper.DrawMotionPresetField(Properties["MotionPreset"]);
                        Properties.Draw("<MotionPivot>k__BackingField");

                        if (motionListHelper != null)
                        {
                            EditorGUILayout.Space();
                            MotionPreset presetInstance = Behaviour.MotionBlender.Instance;
                            motionListHelper.DrawMotionsList(presetInstance);
                        }
                    }
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorDrawing.EndBorderHeaderLayout();
            }
        }
    }
}