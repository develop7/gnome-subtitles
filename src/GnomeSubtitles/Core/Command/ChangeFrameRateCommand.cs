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

using GnomeSubtitles.Ui.View;
using Mono.Unix;

namespace GnomeSubtitles.Core.Command {

public abstract class ChangeFrameRateCommand : FixedMultipleSelectionCommand {
	private float storedFrameRate = 0;

	public ChangeFrameRateCommand (string description, float frameRate) : base(description, false, SelectionIntended.All, true) {
		this.storedFrameRate = frameRate;
	}

	protected override bool ChangeValues () {
		float previousFrameRate = GetFrameRate();
		SetFrameRate(storedFrameRate);
		storedFrameRate = previousFrameRate;
		
		UpdateMenuItem();
		return true;
	}
	
	protected abstract float GetFrameRate ();
	protected abstract void SetFrameRate (float frameRate);
	protected abstract void UpdateMenuItem ();
}

public class ChangeInputFrameRateCommand : ChangeFrameRateCommand {
	private	static string description = Catalog.GetString("Changing Input Frame Rate");

	public ChangeInputFrameRateCommand (float frameRate) : base(description, frameRate) {
	}
	
	protected override float GetFrameRate () {
		return Base.Document.Subtitles.Properties.OriginalFrameRate;
	}
	
	protected override void SetFrameRate (float frameRate) {
		Base.Document.Subtitles.ChangeOriginalFrameRate(frameRate);
	}
	
	protected override void UpdateMenuItem () {
		Base.Ui.Menus.UpdateActiveInputFrameRateMenuItem();
	}
}

public class ChangeVideoFrameRateCommand : ChangeFrameRateCommand {
	private	static string description = Catalog.GetString("Changing Video Frame Rate");

	public ChangeVideoFrameRateCommand (float frameRate) : base(description, frameRate) {
	}
	
	protected override float GetFrameRate () {
		return Base.Document.Subtitles.Properties.CurrentFrameRate;
	}
	
	protected override void SetFrameRate (float frameRate) {
		Base.Document.Subtitles.ChangeFrameRate(frameRate);
	}
	
	protected override void UpdateMenuItem () {
		Base.Ui.Menus.UpdateActiveVideoFrameRateMenuItem();
	}
}

}
