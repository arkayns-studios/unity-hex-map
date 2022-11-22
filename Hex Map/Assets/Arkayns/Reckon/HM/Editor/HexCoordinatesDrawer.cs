﻿using UnityEditor;
using UnityEngine;

namespace Arkayns.Reckon.HM {
	
	[CustomPropertyDrawer(typeof(HexCoordinates))]
	public class HexCoordinatesDrawer : PropertyDrawer {

		// -- Method --
		
		/// <summary> Custom Hex Coordinate inspector drawer </summary>
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			var coordinates = new HexCoordinates(property.FindPropertyRelative("x").intValue, property.FindPropertyRelative("z").intValue);
			position = EditorGUI.PrefixLabel(position, label);
			GUI.Label(position, coordinates.ToString());
		} // Override OnGUI
		
	} // Class HexCoordinatesDrawer
	
} // Namespace Arkayns Reckon HexMap