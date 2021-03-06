using Unity.LEGO.Behaviours;
using UnityEditor;
using UnityEngine;

namespace Unity.LEGO.EditorExt
{
    [CustomEditor(typeof(ControlAction), true)]
    public class ControlActionEditor : MovementActionEditor
    {
        ControlAction m_ControlAction;

        SerializedProperty m_ControlTypeProp;
        SerializedProperty m_InputTypeProp;
        SerializedProperty m_MinSpeedProp;
        SerializedProperty m_MaxSpeedProp;
        SerializedProperty m_IdleSpeedProp;
        SerializedProperty m_RotationSpeedProp;
        SerializedProperty m_JumpSpeedProp;
        SerializedProperty m_MaxJumpsInAirProp;
        SerializedProperty m_IsPlayerProp;
        SerializedProperty m_GravityProp;

        static readonly Color s_BackwardsColour = Color.red;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_ControlAction = (ControlAction)m_Action;

            m_ControlTypeProp = serializedObject.FindProperty("m_ControlType");
            m_InputTypeProp = serializedObject.FindProperty("m_InputType");
            m_MinSpeedProp = serializedObject.FindProperty("m_MinSpeed");
            m_MaxSpeedProp = serializedObject.FindProperty("m_MaxSpeed");
            m_IdleSpeedProp = serializedObject.FindProperty("m_IdleSpeed");
            m_RotationSpeedProp = serializedObject.FindProperty("m_RotationSpeed");
            m_JumpSpeedProp = serializedObject.FindProperty("m_JumpSpeed");
            m_MaxJumpsInAirProp = serializedObject.FindProperty("m_MaxJumpsInAir");
            m_IsPlayerProp = serializedObject.FindProperty("m_IsPlayer");
            m_GravityProp = serializedObject.FindProperty("m_Gravity");
        }

        protected override void CreateGUI()
        {
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);

            EditorGUILayout.PropertyField(m_ControlTypeProp);

            EditorGUILayout.PropertyField(m_InputTypeProp);

            EditorGUI.EndDisabledGroup();

            if ((ControlAction.InputType)m_InputTypeProp.enumValueIndex != ControlAction.InputType.Direct)
            {
                var minSpeedValue = (float)m_MinSpeedProp.intValue;
                var maxSpeedValue = (float)m_MaxSpeedProp.intValue;

                EditorGUILayout.MinMaxSlider(new GUIContent("Speed Range\t(" + m_MinSpeedProp.intValue + " to " + m_MaxSpeedProp.intValue + ")", "The speed in LEGO modules per second."),
                    ref minSpeedValue, ref maxSpeedValue, -50.0f, 50.0f);

                m_MinSpeedProp.intValue = Mathf.RoundToInt(minSpeedValue);
                m_MaxSpeedProp.intValue = Mathf.RoundToInt(maxSpeedValue);

                if (m_MinSpeedProp.intValue != m_MaxSpeedProp.intValue)
                {
                    EditorGUILayout.IntSlider(m_IdleSpeedProp, m_MinSpeedProp.intValue, m_MaxSpeedProp.intValue);
                }
            }
            else
            {
                EditorGUILayout.IntSlider(m_MaxSpeedProp, 0, 50, new GUIContent("Speed", "The speed in LEGO modules per second."));

                if (m_MaxSpeedProp.intValue > 0)
                {
                    EditorGUILayout.IntSlider(m_IdleSpeedProp, 0, m_MaxSpeedProp.intValue);
                }
            }

            EditorGUILayout.PropertyField(m_RotationSpeedProp);

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);

            EditorGUILayout.PropertyField(m_IsPlayerProp);
            EditorGUILayout.PropertyField(m_CollideProp);

            if ((ControlAction.ControlType)m_ControlTypeProp.enumValueIndex == ControlAction.ControlType.Character)
            {
                EditorGUILayout.PropertyField(m_GravityProp);
                EditorGUILayout.PropertyField(m_JumpSpeedProp);
                EditorGUILayout.PropertyField(m_MaxJumpsInAirProp);
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!m_ControlAction.IsPlacedOnBrick());

            if (GUILayout.Button("Focus Camera"))
            {
                EditorUtilities.FocusCamera(m_ControlAction);
            }

            EditorGUI.EndDisabledGroup();
        }

        public override void OnSceneGUI()
        {
            base.OnSceneGUI();

            if (Event.current.type == EventType.Repaint)
            {
                if (m_ControlAction && m_ControlAction.IsPlacedOnBrick())
                {
                    var center = m_ControlAction.GetBrickCenter();
                    var direction = m_ControlAction.GetBrickRotation() * Vector3.forward;
                    var start = center + direction * m_MinSpeedProp.intValue * LEGOBehaviour.LEGOHorizontalModule;
                    var middle = center + direction * m_IdleSpeedProp.intValue * LEGOBehaviour.LEGOHorizontalModule;
                    var end = center + direction * m_MaxSpeedProp.intValue * LEGOBehaviour.LEGOHorizontalModule;

                    if ((ControlAction.InputType)m_InputTypeProp.enumValueIndex == ControlAction.InputType.Direct)
                    {
                        Handles.color = Color.green;
                        Handles.DrawLine(center, end);
                        Handles.DrawSolidDisc(middle, Camera.current.transform.forward, 0.16f);
                        Handles.DrawSolidDisc(end, Camera.current.transform.forward, 0.16f);
                    }
                    else
                    {
                        if (m_MaxSpeedProp.intValue < 0)
                        {
                            Handles.color = s_BackwardsColour;
                            Handles.DrawLine(start, center);
                            Handles.DrawSolidDisc(start, Camera.current.transform.forward, 0.16f);
                            Handles.DrawSolidDisc(middle, Camera.current.transform.forward, 0.16f);
                            Handles.DrawSolidDisc(end, Camera.current.transform.forward, 0.16f);
                        }
                        else if (m_MinSpeedProp.intValue >= 0)
                        {
                            Handles.color = Color.green;
                            Handles.DrawLine(center, end);
                            Handles.DrawSolidDisc(start, Camera.current.transform.forward, 0.16f);
                            Handles.DrawSolidDisc(middle, Camera.current.transform.forward, 0.16f);
                            Handles.DrawSolidDisc(end, Camera.current.transform.forward, 0.16f);
                        }
                        else if (m_IdleSpeedProp.intValue < 0)
                        {
                            Handles.color = s_BackwardsColour;
                            Handles.DrawLine(start, center);
                            Handles.DrawSolidDisc(start, Camera.current.transform.forward, 0.16f);
                            Handles.DrawSolidDisc(middle, Camera.current.transform.forward, 0.16f);
                            Handles.color = Color.green;
                            Handles.DrawLine(center, end);
                            Handles.DrawSolidDisc(end, Camera.current.transform.forward, 0.16f);
                        }
                        else
                        {
                            Handles.color = s_BackwardsColour;
                            Handles.DrawLine(start, center);
                            Handles.DrawSolidDisc(start, Camera.current.transform.forward, 0.16f);
                            Handles.color = Color.green;
                            Handles.DrawLine(center, end);
                            Handles.DrawSolidDisc(middle, Camera.current.transform.forward, 0.16f);
                            Handles.DrawSolidDisc(end, Camera.current.transform.forward, 0.16f);
                        }
                    }
                }
            }
        }
    }
}
