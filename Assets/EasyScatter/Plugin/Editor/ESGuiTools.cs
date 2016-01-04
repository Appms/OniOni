/***********************************************
                    Easy Scatter
	Copyright © 2014-2015 The Hedgehog Team
      http://www.thehedgehogteam.com/Forum/
		
	     The.Hedgehog.Team@gmail.com
		
**********************************************/
using UnityEngine;
using UnityEditor;
using System.Collections;

public class ESGuiTools{

	
	public static void Title(string text,bool bold= true,int width=200){
			
		if (bold){
			text = "<b><size=11>" + text + "</size></b>";
		}
		else{
			text = "<size=11>" + text + "</size>";
		}
		
		GUILayout.Toggle(true,text,"dragtab",GUILayout.Width(width));
			
	}

	public static void SimpleTitle(string text){

		GUIStyle labelStyle = new GUIStyle("label");
		labelStyle.fontStyle = FontStyle.Bold;
		labelStyle.fontSize = 11;
		EditorGUILayout.LabelField(text,labelStyle);
	}


	public static bool FoldOut(string text,bool foldOut, bool bold=true, bool endSpace=true){

		if (bold){
			text = "<b><size=11>" + text + "</size></b>";
		}
		if (foldOut){
			text = "\u25BC " + text;
		}
		else{
			text = "\u25BA " + text;
		}
		
		if ( !GUILayout.Toggle(true,text,"dragtab")){
			foldOut=!foldOut;
		}
		
		if (!foldOut && endSpace)GUILayout.Space(5f);
		
		return foldOut;
	}

	public static bool SimpleFoldOut(string text,bool foldOut, bool bold=true,bool leftAligment=false, bool endSpace=true){

		if (foldOut){
			text = "\u25BC " + text;
		}
		else{
			text = "\u25BA " + text;
		}
		
		if ( !GUILayout.Toggle(true,text,"label")){
			foldOut=!foldOut;
		}
		
		if (!foldOut && endSpace)GUILayout.Space(5f);
		
		return foldOut;
	}
	
	public static bool ToogleButton(bool state,string label, int width=0){

		if (width!=0){
			state = GUILayout.Toggle( state,label,new GUIStyle("Button"), GUILayout.Width(width) ); 
		}
		else{
			state = GUILayout.Toggle( state,label, new GUIStyle("Button") ); 
		}
	
		return state;
	}

	static public bool Button(string label,Color color,int width,int height=0, bool leftAligment=false, Texture2D icon=null ){
		
		GUI.backgroundColor  = color;
		GUIStyle buttonStyle = new GUIStyle("Button");
		
		if (leftAligment)
			buttonStyle.alignment = TextAnchor.MiddleLeft;

		GUIContent guiContent = new GUIContent(label,icon);
		if (height==0){
			if (GUILayout.Button( guiContent,buttonStyle,GUILayout.Width(width))){
				GUI.backgroundColor = Color.white;
				return true;	
			}
		}
		else{
			if (GUILayout.Button( guiContent,buttonStyle,GUILayout.Width(width),GUILayout.Height(height))){
				GUI.backgroundColor = Color.white;
				return true;	
			}			
		}
		GUI.backgroundColor = Color.white;		
		
		return false;
	}

	public static void BeginGroup(int padding=0){
		
		GUILayout.BeginHorizontal();
		GUILayout.Space(padding);
		EditorGUILayout.BeginHorizontal("As TextArea", GUILayout.MinHeight(10f));
		GUILayout.BeginVertical();
		GUILayout.Space(2f);
		
	}

	public static void EndGroup(bool endSpace = true){
		GUILayout.Space(3f);
		GUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(3f);
		GUILayout.EndHorizontal();
		
		if (endSpace){
			GUILayout.Space(10f);
		}
	}
}

		/*
	private static Texture2D gradientTexture;
	private static Texture2D checkerTexture;
	private static Texture2D blueTexture;

	#region Widget
	public static void Title(string text,bool bold= true,int width=200){

		if (bold){
			text = "<b><size=11>" + text + "</size></b>";
		}
		else{
			text = "<size=11>" + text + "</size>";
		}

		GUILayout.Toggle(true,text,"dragtab",GUILayout.Width(width));

	}

	public static bool ButtonTitle(string text,bool bold= true,int width=200){

		bool value = GUILayout.Toggle(true,text,"dragtab",GUILayout.Width(width));

		if (!value){
			return true;
		}
		else{
			return false;
		}
	}

	public static bool ChildFoldOut(bool foldOut,string text, Color color, int width, bool prefixe=true){

		string label = text;
		if (prefixe){
			label="[+] " + text;
			if (foldOut) label="[-] " + text;
		}


		if (GuiTools.Button(label,color,width,true)){
			foldOut = !foldOut;		
		}
		
		return foldOut;
	}

	public static bool BeginFoldOut(string text,bool foldOut, bool endSpace=true){
		
		text = "<b><size=11>" + text + "</size></b>";
		if (foldOut){
			text = "\u25BC " + text;
		}
		else{
			text = "\u25BA " + text;
		}
		
		if ( !GUILayout.Toggle(true,text,"dragtab")){
			foldOut=!foldOut;
		}
		
		if (!foldOut && endSpace)GUILayout.Space(5f);
		
		return foldOut;
	}

	public static void BeginGroup(int padding=0){

		GUILayout.BeginHorizontal();
		GUILayout.Space(padding);
		EditorGUILayout.BeginHorizontal("As TextArea", GUILayout.MinHeight(10f));
		GUILayout.BeginVertical();
		GUILayout.Space(2f);

	}

	public static void EndGroup(bool endSpace = true){
		GUILayout.Space(3f);
		GUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(3f);
		GUILayout.EndHorizontal();
		
		if (endSpace){
			GUILayout.Space(10f);
		}
	}


	public static bool Toggle(string text, bool value,bool leftToggle=false, int width=-1, bool bold=false){


		if (value) GUI.backgroundColor = Color.green; else GUI.backgroundColor = Color.red;

		GUIStyle boldStyle = new GUIStyle( "toggle");
		if (bold)boldStyle.fontStyle = FontStyle.Bold;

		if (leftToggle){
			if (width==-1)
				value = EditorGUILayout.ToggleLeft(text,value);
			else
				value = GUILayout.Toggle(value,text,boldStyle,GUILayout.Width(width));
		}
		else{
			if (width==-1)
				value = EditorGUILayout.Toggle(text,value);
			else
				value = GUILayout.Toggle(value,text,boldStyle,GUILayout.Width(width));
		}
		
		GUI.backgroundColor = Color.white;


		return value;
		
	}

	public static bool BeginToogleGroup(string text, bool value,int indent=0){
		if (value) GUI.backgroundColor = Color.green; else GUI.backgroundColor = Color.red;
		value = EditorGUILayout.BeginToggleGroup( text,value);
		GUI.backgroundColor = Color.white;

		if (value)
			GuiTools.BeginGroup(indent);

		return value;
	}

	public static void EndToggleGroup( bool value,bool space=false){
		if (value)
			GuiTools.EndGroup(space);

		EditorGUILayout.EndToggleGroup();
	}


	static public bool Button(string label,Color color,int width, bool leftAligment=false, int height=0){
		
		GUI.backgroundColor  = color;
		GUIStyle buttonStyle = new GUIStyle("Button");
		
		if (leftAligment)
			buttonStyle.alignment = TextAnchor.MiddleLeft;
		
		if (height==0){
			if (GUILayout.Button( label,buttonStyle,GUILayout.Width(width))){
				GUI.backgroundColor = Color.white;
				return true;	
			}
		}
		else{
			if (GUILayout.Button( label,buttonStyle,GUILayout.Width(width),GUILayout.Height(height))){
				GUI.backgroundColor = Color.white;
				return true;	
			}			
		}
		GUI.backgroundColor = Color.white;		
		
		return false;
	}


	static public float Stepper(string label,float value, float minValue, float stepValue, float maxStep, string postLabel="", bool isFloating=false){

		float step = (value-minValue)/stepValue;
		 
		GUILayout.Space(16);
		Rect lastRect = GUILayoutUtility.GetLastRect();

		// Increase
		lastRect.x = 20;
		if (GUI.Button( new Rect(lastRect.x,lastRect.y,20,16 ),"-")){
			step--;
			if (step< 0) step++;
		}

		// Decrease
		lastRect.x = 40;
		lastRect.width = Screen.width-84;
		if (GUI.Button( new Rect(Screen.width-44,lastRect.y,20,16 ),"+")){
			step++;
			if (step>maxStep) step--;
		}

		// indicateur
		//EditorGUI.ProgressBar( lastRect,1f/maxStep * step,label + " level : " + (step+1).ToString("f0"));
		float returnValue = minValue + (stepValue * step);
		if (!isFloating){
			EditorGUI.ProgressBar( lastRect,1f/maxStep * step,label + " : " + returnValue.ToString("f0") +  postLabel);
		}
		else{
			EditorGUI.ProgressBar( lastRect,1f/maxStep * step,label + " : " + returnValue.ToString("f1") +  postLabel);
		}

		GUILayout.Space(5);

		return returnValue;

	}

	static public void ProgressBar(string label,float value, float maxValue){
		GUILayout.Space(16);
		Rect lastRect = GUILayoutUtility.GetLastRect();
		lastRect.width = Screen.width-84;
		lastRect.x = 40;
		EditorGUI.ProgressBar( lastRect, value/maxValue, label);
	}

	static public int Incrementer(string label,int count, int min=0, int max=100){

		EditorGUILayout.LabelField( label);
		Rect lastRect = GUILayoutUtility.GetLastRect();

		lastRect.x = 150;
		if (GUI.Button( new Rect(Screen.width-44-32-20, lastRect.y,20,16),"-")){
			count--;	
		}
		count = EditorGUI.IntField( new Rect( Screen.width-44-32, lastRect.y,32,16 ),count);  
		
		if (GUI.Button( new Rect(Screen.width-44, lastRect.y,20,16),"+")){
			count++;	
		}			

		
		count = Mathf.Clamp(count,min,max);
		return count;
	}
	#endregion

	#region 2d effect
	public static void DrawSeparatorLine(int padding=0){
		
		EditorGUILayout.Space();
		DrawLine(Color.gray, padding);
		EditorGUILayout.Space();
	}

	private static void DrawLine(Color color,int padding=0){
		
		GUILayout.Space(10);
		Rect lastRect = GUILayoutUtility.GetLastRect();
		
		GUI.color = color;
		GUI.DrawTexture(new Rect(padding, lastRect.yMax -lastRect.height/2f, Screen.width, 1f), EditorGUIUtility.whiteTexture);
		GUI.color = Color.white;
	}
	#endregion

	#region Texture
	public static void DrawTextureRectPreview( Rect rect, Rect textureRect, Texture2D tex, Color color){
		
		GUI.color = color;
		GUI.DrawTexture( rect, EditorGUIUtility.whiteTexture);
		GUI.color = Color.white;
		
		rect = new Rect(rect.x+2,rect.y+2,rect.width-4,rect.height-4);
		DrawTileTexture( rect, GuiTools.GetCheckerTexture());

		if (tex!=null){		
			GUI.DrawTextureWithTexCoords( rect, tex,textureRect );
		}
		
	}

	public static void DrawTileTexture (Rect rect, Texture tex)
	{
		GUI.BeginGroup(rect);
		{
			int width  = Mathf.RoundToInt(rect.width);
			int height = Mathf.RoundToInt(rect.height);
			
			for (int y = 0; y < height; y += tex.height)
			{
				for (int x = 0; x < width; x += tex.width)
				{
					GUI.DrawTexture(new Rect(x, y, tex.width, tex.height), tex);
				}
			}
		}
		GUI.EndGroup();
	}

	private static Rect DrawGradient(int padding, int width, int height=35){
		
		GUILayout.Space(height);
		Rect lastRect = GUILayoutUtility.GetLastRect();
		lastRect.yMin = lastRect.yMin + 7;
		lastRect.yMax = lastRect.yMax - 7;
		lastRect.width =  Screen.width;
		
		GUI.DrawTexture(new Rect(padding,lastRect.yMin+1,width, lastRect.yMax- lastRect.yMin), GetGradientTexture());
		
		return lastRect;
	}

	public static Texture2D GetBlueTexture(){

			if (blueTexture == null){
				blueTexture = CreateBlueTexture();
			}
			return blueTexture;
	}

	private static Texture2D GetGradientTexture(){
		
		if (gradientTexture==null){
			gradientTexture = CreateGradientTexture();
		}
		return gradientTexture;
		
	}

	private static Texture2D CreateBlueTexture(){

			Texture2D myTexture = new Texture2D(1, 1);
			myTexture.hideFlags = HideFlags.HideInInspector;
			myTexture.hideFlags = HideFlags.DontSave;
			myTexture.filterMode = FilterMode.Bilinear;

			myTexture.SetPixel(0, 0, new Color(62f/255f,125f/255f,231f/255f));

			myTexture.Apply();
			
			return myTexture;
	}

	private static Texture2D CreateGradientTexture(){
		
		int height =18;
		
		Texture2D myTexture = new Texture2D(1, height);
		myTexture.hideFlags = HideFlags.HideInInspector;
		myTexture.hideFlags = HideFlags.DontSave;
		myTexture.filterMode = FilterMode.Bilinear;
		
		Color startColor= new Color(0.4f,0.4f,0.4f);
		Color endColor= new Color(0.6f,0.6f,0.6f);
		
		float stepR = (endColor.r - startColor.r)/18f;
		float stepG = (endColor.g - startColor.g)/18f;
		float stepB = (endColor.b - startColor.b)/18f;
		
		Color pixColor = startColor;
		
		for (int i = 1; i < height-1; i++)
		{
			pixColor = new Color(pixColor.r + stepR,pixColor.g + stepG , pixColor.b + stepB);
			myTexture.SetPixel(0, i, pixColor);
		}
		
		myTexture.SetPixel(0, 0, new Color(0,0,0));
		myTexture.SetPixel(0, 17, new Color(1,1,1));
		
		myTexture.Apply();
		
		return myTexture;
	}

	public static Texture2D GetCheckerTexture(){
		if (GuiTools.checkerTexture==null){
			GuiTools.checkerTexture = CreateCheckerTexture();
		}
		return checkerTexture;		
	}

	private static Texture2D CreateCheckerTexture(){
		
		Texture2D myTexture = new Texture2D(16, 16);
		myTexture.hideFlags = HideFlags.DontSave;
		
		Color color1 = new Color(0.5f,0.5f,0.5f);
		for( int x=0;x<8;x++){
			for( int y=0;y<8;y++){
				myTexture.SetPixel(x, y, color1);
				myTexture.SetPixel(x+8, y+8, color1);
			}
		}
		
		color1 = new Color(0.3f,0.3f,0.3f);
		for( int x=0;x<8;x++){
			for( int y=0;y<8;y++){
				myTexture.SetPixel(x+8, y, color1);
				myTexture.SetPixel(x, y+8, color1);
			}
		}
		
		myTexture.Apply();
		
		return myTexture;
	}	
	#endregion

*/
