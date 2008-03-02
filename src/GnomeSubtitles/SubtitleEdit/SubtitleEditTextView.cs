/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2008 Pedro Castro
 *
 * Gnome Subtitles is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * Gnome Subtitles is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

using Gtk;
using SubLib;
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace GnomeSubtitles {

public abstract class SubtitleEditTextView {
	private TextView textView = null;
	private bool isBufferChangeSilent = false; //used to indicate whether a buffer change should set the subtitle text in the subtitle list
	private bool isBufferInsertManual = false; //used to indicate whether there were manual (not by the user) inserts to the buffer
	private bool isBufferDeleteManual = false; //used to indicate whether there were manual (not by the user) inserts to the buffer
	private bool isToggleOverwriteSilent = false; //used to indicate whether an overwrite toggle was manual
	
	/* Text tags */
	private TextTag scaleTag = new TextTag("scale");
	private TextTag boldTag = new TextTag("bold");
	private TextTag italicTag = new TextTag("italic");
	private TextTag underlineTag = new TextTag("underline");
	private ArrayList subtitleTags = new ArrayList(4); //4 not to resize container with 3 items added
	
	/* Other */
	private Subtitle subtitle = null;
	private IntPtr spellTextView = IntPtr.Zero;


	public SubtitleEditTextView (TextView textView) {
		this.textView = textView;

		/* Init tags */
		scaleTag.Scale = Pango.Scale.XLarge;
    	boldTag.Weight = Pango.Weight.Bold;
    	italicTag.Style = Pango.Style.Italic;
    	underlineTag.Underline = Pango.Underline.Single;

		/* Init text view */
		textView.Buffer.TagTable.Add(scaleTag);
    	textView.Buffer.TagTable.Add(boldTag);
    	textView.Buffer.TagTable.Add(italicTag);
    	textView.Buffer.TagTable.Add(underlineTag);

    	ConnectSignals();
	}
	
	/* Abstract members */
	
	protected abstract SubtitleTextType GetTextType ();
	protected abstract void ChangeSubtitleTextContent (Subtitle subtitle, string text);
	protected abstract string GetSubtitleTextContent (Subtitle subtitle);
	protected abstract void ExecuteInsertCommand (int index, string insertion);
	protected abstract void ExecuteDeleteCommand (int index, string deletion, int cursor);
	
	/* Events */
	public event EventHandler ToggleOverwrite = null;
		
	/* Public properties */

	public TextView TextView {
		get { return textView; }
	}
	
	public bool Enabled {
		get { return textView.Sensitive; }
	}
	
    public bool IsFocus {
    	get { return textView.IsFocus; }    
    }
    
    /// <summary>The visibility of the scrolled window this <see cref="TextView" /> is inside of.</summary>
    public bool Visible {
    	set { Global.GetWidget(WidgetNames.SubtitleEditTranslationScrolledWindow).Visible = value; }
    }
    
    public bool OverwriteStatus {
    	get { return textView.Overwrite; }
    }
	
	/// <summary>The text that is currently selected, or <see cref="Selection.Empty" /> if no text is selected.</summary>
    public string Selection {
    	get {
    		if (!this.Enabled)
    			return String.Empty;

    		TextIter start, end;
    		if (textView.Buffer.GetSelectionBounds(out start, out end)) //has selection
    			return textView.Buffer.GetText(start, end, false);
    		else
    			return String.Empty;
    	}
    }
	
	/* Public methods */
	
	public void ClearFields () {
		SetText(String.Empty);	
	}
		
	public void LoadSubtitle (Subtitle subtitle) {
		this.subtitle = subtitle;
		LoadTags(subtitle.Style);
		SetText(GetSubtitleTextContent(subtitle));
	}

	public void InsertText (int startIndex, string text) {
		TextBuffer buffer = textView.Buffer;
		isBufferInsertManual = true;

		buffer.BeginUserAction();
		GrabFocus();
		PlaceCursor(startIndex);
		buffer.InsertAtCursor(text);
		FocusOnSelection(startIndex, startIndex + text.Length);
    	buffer.EndUserAction();

    	isBufferInsertManual = false;
	}
	
	public void DeleteText (int startIndex, int endIndex) {
		TextBuffer buffer = textView.Buffer;
		isBufferDeleteManual = true;
		
		buffer.BeginUserAction();
		GrabFocus();
		TextIter start = buffer.GetIterAtOffset(startIndex);
		TextIter end = buffer.GetIterAtOffset(endIndex);
		buffer.Delete(ref start, ref end);
		buffer.EndUserAction();

		isBufferDeleteManual = false;
	}

	public void FocusOnSelection (int startIndex, int endIndex) {
    	GrabFocus();
		TextIter start = textView.Buffer.GetIterAtOffset(startIndex);
		TextIter end = textView.Buffer.GetIterAtOffset(endIndex);
		textView.Buffer.SelectRange(start, end);		
    }
	
	public void ReplaceSelection (string replacement) {
    	TextBuffer buffer = textView.Buffer;
    	buffer.BeginUserAction();
    	buffer.DeleteSelection(true, true);
    	buffer.InsertAtCursor(replacement);
    	buffer.EndUserAction();    
    }
    
    /// <summary>Gets the bounds of the current selection, if text is selected.</summary>
	/// <param name="start">The start index of the selection.</param>
	/// <param name="end">The end index of the selection.</param>
	/// <remarks>If no text is selected, both start and end will contain the index of the cursor position.</remarks>
	/// <returns>Whether text was selected.</returns>
	public bool GetTextSelectionBounds (out int start, out int end) {
		TextIter startIter, endIter;
		if (textView.Buffer.GetSelectionBounds(out startIter, out endIter)) { //has selection
			start = startIter.Offset;
			end = endIter.Offset;
			return true;
		}
		else {
			int cursorIndex = GetCursorIndex();
			start = cursorIndex;
			end = cursorIndex;
			return false;
		}
	}
    
    /// <summary>Toggles the overwrite status without emitting its event.</summary>
    public void ToggleOverwriteSilent () {
    	isToggleOverwriteSilent = true;
    	textView.Overwrite = !textView.Overwrite;
    	isToggleOverwriteSilent = false;
    }

    /* GtkSpell */
	[DllImport ("libgtkspell.so.0")]
	static extern IntPtr gtkspell_new_attach (IntPtr textView, string locale, IntPtr error);

	[DllImport ("libgtkspell.so.0")]
	static extern void gtkspell_detach (IntPtr obj);

	[DllImport ("libgtkspell.so.0")]
	static extern bool gtkspell_set_language (IntPtr textView, string lang, IntPtr error);
	
	private void GtkSpellDetach () {
		if (spellTextView != IntPtr.Zero)
			gtkspell_detach(spellTextView);
	}
	
	private void GtkSpellAttach () {
		spellTextView = gtkspell_new_attach(textView.Handle, null, IntPtr.Zero);
	}
	
	private bool GtkSpellSetLanguage (string language) {
		return gtkspell_set_language(spellTextView, "asdasd", IntPtr.Zero);
	}
	

	/* Private methods */

    private void SetText (string text) {
    	isBufferChangeSilent = true;
    	isBufferInsertManual = true;
    	isBufferDeleteManual = true;
    	
    	textView.Buffer.Text = text;
    	
    	isBufferChangeSilent = false;
    	isBufferInsertManual = false;
    	isBufferDeleteManual = false;
    }

    private void SetTag (TextTag tag, TextIter start, TextIter end, bool activate) {
		if (activate)
			textView.Buffer.ApplyTag(tag, start, end);
		else
			textView.Buffer.RemoveTag(tag, start, end);
    }
    
	private void LoadTags (SubLib.Style style) {
    	subtitleTags.Clear();
    	if (style.Bold)
    		subtitleTags.Add(boldTag);
    	if (style.Italic)
    		subtitleTags.Add(italicTag);
    	if (style.Underline)
    		subtitleTags.Add(underlineTag);
    }
    
    private void ApplyLoadedTags () {
    	TextBuffer buffer = textView.Buffer;
    	TextIter start = buffer.StartIter;
    	TextIter end = buffer.EndIter;
    	buffer.ApplyTag(scaleTag, start, end);
    	foreach (TextTag tag in subtitleTags)
			SetTag(tag, start, end, true);
    }
    
    private TextIter GetIterAtInsertMark () {
    	return textView.Buffer.GetIterAtMark(textView.Buffer.InsertMark);
    }
    
    private void GetLineColumn (out int line, out int column) {
    	TextIter iter = GetIterAtInsertMark();
		line = iter.Line + 1;
		column = iter.LineOffset + 1;
    }
    
	private void UpdateLineColStatus () {
    	if ((!Enabled) || (!IsFocus))
    		return;

		/* Get the cursor position */
		int line, column;
		GetLineColumn(out line, out column);
		
		/* Update the status bar */
		Global.GUI.Status.SetPosition(GetTextType(), line, column);
	}
    
	private void UpdateOverwriteStatus () {
		Global.GUI.Status.Overwrite = textView.Overwrite;
	}
	
	private void PlaceCursor (int index) {
		TextIter iter = textView.Buffer.GetIterAtOffset(index);
		textView.Buffer.PlaceCursor(iter);
	}
	
	/// <summary>Returns the cursor index, or -1 if the text view is not enabled.</summary>
    private int GetCursorIndex () {
    	if (!this.Enabled)
    		return -1;
    	else {
    		TextIter iter = GetIterAtInsertMark();
    		return iter.Offset;
    	}
    }

	private void GrabFocus () {
		textView.GrabFocus();
	}
	
	
	/* Event methods */

	private void OnBufferChanged (object o, EventArgs args) {
		if (!isBufferChangeSilent)
			ChangeSubtitleTextContent(subtitle, textView.Buffer.Text);
		
		ApplyLoadedTags();
		UpdateLineColStatus();
	}
	
	private void OnBufferMarkSet (object o, MarkSetArgs args) {
		UpdateLineColStatus();
	}
	
	[GLib.ConnectBefore]
	private void OnBufferInsertText (object o, InsertTextArgs args) {
		if (!isBufferInsertManual) {
			int index = args.Pos.Offset;
			string insertion = args.Text;
			ExecuteInsertCommand(index, insertion);
		}
		
		ApplyLoadedTags();		
		UpdateLineColStatus();
	}
	
	[GLib.ConnectBefore]
	private void OnBufferDeleteRange (object o, DeleteRangeArgs args) {
		if (!isBufferDeleteManual) {
			int index = args.Start.Offset;
			int length = args.End.Offset - index;
			string deletion = textView.Buffer.Text.Substring(index, length);
			ExecuteDeleteCommand(index, deletion, GetCursorIndex()); 
		}
	}
	
    private void OnFocusIn (object o, FocusInEventArgs args) {
    	UpdateLineColStatus();
		UpdateOverwriteStatus();
		
		Global.GUI.Menus.SetPasteSensitivity(true);
	}
	
	private void OnFocusOut (object o, FocusOutEventArgs args) {
		Global.GUI.Menus.SetPasteSensitivity(false);
    	Global.GUI.Status.ClearEditRelatedStatus();
	}
	
	private void OnToggleOverwrite (object o, EventArgs args) {
		/* Update the GUI overwrite status */
    	UpdateOverwriteStatus();
	
		/* Emit the toggle event */
		if (!isToggleOverwriteSilent)
			EmitToggleOverwrite();
	}
	
	private void OnSpellToggleEnabled (object o, EventArgs args) {
		bool enabled = Global.SpellLanguages.Enabled;
		if (enabled) {
			GtkSpellAttach();
			string language = Global.SpellLanguages.ActiveLanguage;
			GtkSpellSetLanguage(language);
		}
		else
			GtkSpellDetach();
	}
	
	private void OnSpellLanguageChanged (object o, EventArgs args) {
		if (Global.SpellLanguages.Enabled) {
			string language = Global.SpellLanguages.ActiveLanguage;
			GtkSpellSetLanguage(language);
		}
	}
	
	private void OnDestroyed (object o, EventArgs args) {
		GtkSpellDetach();
	}
	
	[GLib.ConnectBefore]
    private void OnKeyPressed (object o, KeyPressEventArgs args) {
    	Gdk.Key key = args.Event.Key;
    	Gdk.ModifierType modifier = args.Event.State;
    	Gdk.ModifierType controlModifier = Gdk.ModifierType.ControlMask;
    	
    	if ((modifier & controlModifier) == controlModifier) { //Control was pressed
    		switch (key) {
    			case Gdk.Key.Page_Up:
    				Global.GUI.View.Selection.SelectPrevious();
					GrabFocus();
    				args.RetVal = true;
    				break;
    			case Gdk.Key.Page_Down:
					Global.GUI.View.Selection.SelectNext();
					GrabFocus();
    				args.RetVal = true;
    				break;
    		}
    	}
    }

    private void ConnectSignals () {
		/* Buffer signals */
		textView.Buffer.Changed += OnBufferChanged;
		textView.Buffer.MarkSet += OnBufferMarkSet;
		textView.Buffer.InsertText += OnBufferInsertText;
		textView.Buffer.DeleteRange += OnBufferDeleteRange;
		
		/* TextView signals */
		textView.FocusInEvent += OnFocusIn;
		textView.FocusOutEvent += OnFocusOut;
		textView.KeyPressEvent += OnKeyPressed;
		textView.ToggleOverwrite += OnToggleOverwrite;
		TextView.Destroyed += OnDestroyed;
		
		/* Spell signals */
		Global.SpellLanguages.ToggleEnabled += OnSpellToggleEnabled;
		Global.SpellLanguages.LanguageChanged += OnSpellLanguageChanged;
    }
    
    private void EmitToggleOverwrite () {
    	if (this.ToggleOverwrite != null)
    		this.ToggleOverwrite(this, EventArgs.Empty);
    }


}

}
