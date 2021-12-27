using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

public class PixelDissolveRevealShaderGUI : ShaderGUI 
{
	public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		base.OnGUI(materialEditor, properties);

		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		BeginBox();
		{
			OnGUI_DrawShaderProperty("_MainTex", "Main Texture", materialEditor, properties);
			OnGUI_DrawShaderProperty("_MainColor", "Main Color", materialEditor, properties);
		}
		EndBox();

		BeginBox();
		{
			OnGUI_DrawShaderProperty("_DissolveNoise", "Dissolve Noise Texture", materialEditor, properties);
			OnGUI_DrawShaderProperty("_DissolveColor", "Dissolve Color", materialEditor, properties);
			OnGUI_DrawShaderProperty("_DistortionLevel", "Distortion Level", materialEditor, properties);
			OnGUI_DrawShaderProperty("_DissolveIntensity", "Dissolve Intensity", materialEditor, properties);
			OnGUI_DrawShaderProperty("_DissolveThickness", "Dissolve Thickness", materialEditor, properties);
			OnGUI_DrawShaderProperty("_DissolveThickness2", "Dissolve Thickness 2", materialEditor, properties);
			OnGUI_DrawShaderProperty("_DissolvePixelLevel", "Dissolve Pixel Level", materialEditor, properties);
		}
		EndBox();

		BeginBox();
		{
			OnGUI_DrawShaderProperty("_Wind", "Wind", materialEditor, properties);
			OnGUI_DrawShaderProperty("_Speed", "Speed", materialEditor, properties);
		}
		EndBox();
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
