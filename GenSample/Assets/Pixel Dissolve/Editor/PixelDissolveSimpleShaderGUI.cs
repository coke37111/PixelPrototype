using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

public class PixelDissolveSimpleShaderGUI : ShaderGUI 
{
	public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		base.OnGUI(materialEditor, properties);

		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		OnGUI_CullMode(materialEditor, properties);

		BeginBox();
		{
			OnGUI_DrawShaderProperty("_MainTex", "Main Texture", materialEditor, properties);
			OnGUI_DrawShaderProperty("_MainColor", "Main Color", materialEditor, properties);
			OnGUI_DrawShaderProperty("_TexCutoff", "Main Ratio", materialEditor, properties);
		}
		EndBox();

		BeginBox();
		{
			OnGUI_DrawShaderProperty("_DissolveNoise", "Dissolve Noise Texture", materialEditor, properties);
			OnGUI_DrawShaderProperty("_DissolveColor", "Dissolve Color", materialEditor, properties);
			OnGUI_DrawShaderProperty("_GlowCutoff", "Dissolve Ratio", materialEditor, properties);
			OnGUI_Direction(materialEditor, properties);
		}
		EndBox();

		BeginBox();
		{
			OnGUI_DrawShaderProperty("_Wind", "Wind", materialEditor, properties);
			OnGUI_DrawShaderProperty("_Speed", "Speed", materialEditor, properties);
			OnGUI_DrawShaderProperty("_Start", "Start", materialEditor, properties);
			OnGUI_DrawShaderProperty("_End", "End", materialEditor, properties);
			OnGUI_DrawShaderProperty("_PixelLevel", "Pixel Level", materialEditor, properties);
		}
		EndBox();
	}

	private void OnGUI_Direction(MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		MaterialProperty mp_directionEnum = FindProperty("_DirectionEnum", properties);
		EditorGUI.BeginChangeCheck();
		materialEditor.ShaderProperty(mp_directionEnum, "Dissolve Direction");
		if(EditorGUI.EndChangeCheck())
		{
			MaterialProperty mp_direction = FindProperty("_Direction", properties);
			if(mp_directionEnum.floatValue == 0)
			{
				mp_direction.vectorValue = new Vector4(1,0,0,0);
			}
			else if(mp_directionEnum.floatValue == 1)
			{
				mp_direction.vectorValue = new Vector4(0,1,0,0);
			}
			else
			{
				mp_direction.vectorValue = new Vector4(0,0,1,0);
			}
			materialEditor.serializedObject.ApplyModifiedProperties();
		}
	}

	private void OnGUI_CullMode(MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		MaterialProperty mp = FindProperty("_Cull", properties);
		CullMode cullMode = (CullMode)mp.floatValue;

		EditorGUI.BeginChangeCheck();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Cull Mode");
		cullMode = (CullMode)EditorGUILayout.EnumPopup(cullMode);
		EditorGUILayout.EndHorizontal();
		if (EditorGUI.EndChangeCheck())
		{
			materialEditor.RegisterPropertyChangeUndo("Culling Mode");
			mp.floatValue = (float)cullMode;
			materialEditor.serializedObject.ApplyModifiedProperties();
		}
	}

	public static bool OnGUI_DrawShaderProperty(string propertyName, string label, MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        MaterialProperty mp = FindProperty(propertyName, properties, false);
        if(mp != null)
        {
            materialEditor.ShaderProperty(mp, label);
        }
        return mp != null;
    }

	public static void BeginBox()
	{
		EditorGUILayout.BeginVertical(GUI.skin.GetStyle("Box"));
		EditorGUILayout.Space();
	}

	public static void EndBox()
	{
		EditorGUILayout.Space();
		EditorGUILayout.EndVertical();
	}
}
