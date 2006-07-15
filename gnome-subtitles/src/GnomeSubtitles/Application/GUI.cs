/*
 * This file is part of Gnome Subtitles, a subtitle editor for Gnome.
 * Copyright (C) 2006 Pedro Castro
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

using Gnome;
using Gtk;
using System;
using System.Text;
using SubLib;

namespace GnomeSubtitles {

public class GUI : GladeWidget {
	private ApplicationCore core = null;
	private App window = null;
	private Menus menus = null;
	private SubtitleView subtitleView = null;	
	private SubtitleEdit subtitleEdit = null;
	
	public GUI () {
		core = new ApplicationCore(this);
		Init(ExecutionInfo.GladeMasterFileName, WidgetNames.MainWindow, core.Handlers);
		core.Handlers.Init(this.Glade);
		
		window = (App)GetWidget(WidgetNames.MainWindow);
		subtitleView = new SubtitleView(this, this.Glade);
		subtitleEdit = new SubtitleEdit(this, this.Glade);
		menus = new Menus(this, this.Glade);
		
		if (ExecutionInfo.Args.Length > 0)
			Open(ExecutionInfo.Args[0]);
		else
			menus.SetStartUpSensitivity();
			
		core.Program.Run();
    }
      
	public ApplicationCore Core {
		get { return core; }
	}
	
	public App Window {
		get { return window; }
	}

	public Menus Menus {
		get { return menus; }
	}
	
	public SubtitleView SubtitleView {
		get { return subtitleView; }
	}
	
	public SubtitleEdit SubtitleEdit {
		get { return subtitleEdit; }
	}
    
    
    
    public void Close() {
    	core.Program.Quit();
    }

    public void New () {
    	core.New();
    	NewDocument();
    }
    
    public void Open (string fileName) {
   	 	core.Open(fileName);
    	NewDocument();
    }
    
    public void Open (string fileName, Encoding encoding) {
		core.Open(fileName, encoding);
    	NewDocument();
    }
    
    public void Save () {
    	core.Subtitles.Save();
    	core.CommandManager.WasModified = false;
    	SetWindowTitle(false);
    }
    
    public void SaveAs (string filePath, SubtitleType subtitleType, Encoding encoding) {
		core.Subtitles.SaveAs(filePath, subtitleType, encoding);
		menus.SetActiveTimingMode();
		SetWindowTitle(false);
    }
	
	public void SetTimingMode (TimingMode mode) {
		core.Subtitles.Properties.TimingMode = mode;
		subtitleView.UpdateTimingMode();
		subtitleEdit.UpdateTimingMode();
	}
	
	public void SetWindowTitle (bool modified) {
		string prefix = (modified ? "*" : String.Empty);
		window.Title = prefix + core.Subtitles.Properties.FileName +
			" - " + ExecutionInfo.ApplicationName;
	}
	
	public void OnSubtitleSelection (Subtitle subtitle) {
		menus.SetActiveStyles(subtitle.Style.Bold, subtitle.Style.Italic, subtitle.Style.Underline);
		subtitleEdit.LoadSubtitle(subtitle);
	}
	
	public void OnSubtitleSelection (TreePath[] paths) {
		bool bold, italic, underline;
		GetGlobalStyles(paths, out bold, out italic, out underline);
		menus.SetActiveStyles(bold, italic, underline);
		subtitleEdit.Sensitive = false;
	}
	
	public void RefreshAndReselect () {
		subtitleView.Refresh();
		subtitleView.Reselect();
	}
    
    
    /* Private methods */
    
	private void NewDocument () {
		menus.SetNewDocumentSensitivity();
		menus.SetActiveTimingMode();
		menus.SetFrameRateMenus();
		SetWindowTitle(false);

		subtitleView.SetUp();
		subtitleView.Load(core.Subtitles);
		subtitleEdit.SetUp();
		
		subtitleView.SelectFirst();
	}
	
	
	private void GetGlobalStyles (TreePath[] paths, out bool bold, out bool italic, out bool underline) {
		Subtitles subtitles = core.Subtitles;
		bold = true;
		italic = true;
		underline = true;
		foreach (TreePath path in paths) {
			Subtitle subtitle = subtitles.Get(path);
			if ((bold == true) && !subtitle.Style.Bold) //bold hasn't been unset
				bold = false;
			if ((italic == true) && !subtitle.Style.Italic)
				italic = false;
			if ((underline == true) && !subtitle.Style.Underline)
				underline = false;
		}		
	}
	

	



}

}
