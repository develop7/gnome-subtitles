/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2010 Pedro Castro
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

using GConf;
using System;

namespace GnomeSubtitles.Core {

/* Enumerations */
public enum ConfigFileOpenEncodingOption { AutoDetect = 0, RememberLastUsed = 1, CurrentLocale = 3, Specific = 4 }; //Values match ordering where the options are used
public enum ConfigFileOpenEncoding { AutoDetect = 0, CurrentLocale = 2, Fixed = 3 };
public enum ConfigFileOpenFallbackEncoding { CurrentLocale = 0, Fixed = 1 };
public enum ConfigFileSaveEncodingOption { KeepExisting = 0, RememberLastUsed = 1, CurrentLocale = 3, Specific = 4 }; //Values match ordering where the options are used
public enum ConfigFileSaveEncoding { KeepExisting = -1, CurrentLocale = 0, Fixed = 1 }; //KeepExisting=-1 because it doesn't appear

public class Config {
	private Client client = null;
	
	/* Constant prefix strings */
	private const string keyPrefix = "/apps/gnome-subtitles/";
	private const string keyPrefs = keyPrefix + "preferences/";
	private const string keyPrefsEncodings = keyPrefs + "encodings/";
	private const string keyPrefsSpellCheck = keyPrefs + "spellcheck/";
	private const string keyPrefsVideo = keyPrefs + "video/";
	private const string keyPrefsWindow = keyPrefs + "window/";
	private const string keyPrefsDefaults = keyPrefs + "defaults/";

	/* Constant key strings */
	private const string keyPrefsEncodingsShownInMenu = keyPrefsEncodings + "shown_in_menu";
	private const string keyPrefsSpellCheckActiveTextLanguage = keyPrefsSpellCheck + "active_text_language";
	private const string keyPrefsSpellCheckActiveTranslationLanguage = keyPrefsSpellCheck + "active_translation_language";
	private const string keyPrefsSpellCheckAutocheck = keyPrefsSpellCheck + "autocheck";
	private const string keyPrefsVideoAutoChooseFile = keyPrefsVideo + "auto_choose_file";
	private const string keyPrefsWindowHeight = keyPrefsWindow + "height";
	private const string keyPrefsWindowWidth = keyPrefsWindow + "width";
	private const string keyPrefsDefaultsFileOpenEncodingOption = keyPrefsDefaults + "file_open_encoding_option";
	private const string keyPrefsDefaultsFileOpenEncoding = keyPrefsDefaults + "file_open_encoding";
	private const string keyPrefsDefaultsFileOpenFallbackEncoding = keyPrefsDefaults + "file_open_fallback";
	private const string keyPrefsDefaultsFileSaveEncodingOption = keyPrefsDefaults + "file_save_encoding_option";
	private const string keyPrefsDefaultsFileSaveEncoding = keyPrefsDefaults + "file_save_encoding";

	public Config () {
		client = new Client();
	}
	
	/* Public properties */

	public string[] PrefsEncodingsShownInMenu {
		get {
			string[] defaultValue = { "ISO-8859-15" };
			return GetStrings(keyPrefsEncodingsShownInMenu, defaultValue);
		}
		set { SetStrings(keyPrefsEncodingsShownInMenu, value); }
	}
	
	public string PrefsSpellCheckActiveTextLanguage {
		get { return GetString(keyPrefsSpellCheckActiveTextLanguage, String.Empty); }
		set { Set(keyPrefsSpellCheckActiveTextLanguage, value); }
	}
	
	public string PrefsSpellCheckActiveTranslationLanguage {
		get { return GetString(keyPrefsSpellCheckActiveTranslationLanguage, String.Empty); }
		set { Set(keyPrefsSpellCheckActiveTranslationLanguage, value); }
	}
	
	public bool PrefsSpellCheckAutocheck {
		get { return GetBool(keyPrefsSpellCheckAutocheck, false); }
		set { Set(keyPrefsSpellCheckAutocheck, value); }
	}
	
	public bool PrefsVideoAutoChooseFile {
		get { return GetBool(keyPrefsVideoAutoChooseFile, true); }
		set { Set(keyPrefsVideoAutoChooseFile, value); }
	}
	
	public int PrefsWindowHeight {
		get { return GetInt(keyPrefsWindowHeight, 600, 200, true, 0, false); }
		set { Set(keyPrefsWindowHeight, value); }
	}
	
	public int PrefsWindowWidth {
		get { return GetInt(keyPrefsWindowWidth, 690, 200, true, 0, false); }
		set { Set(keyPrefsWindowWidth, value); }
	}

	public ConfigFileOpenEncodingOption PrefsDefaultsFileOpenEncodingOption {
		get { return (ConfigFileOpenEncodingOption)GetEnumValue(keyPrefsDefaultsFileOpenEncodingOption, ConfigFileOpenEncodingOption.AutoDetect); }
		set { Set(keyPrefsDefaultsFileOpenEncodingOption, value.ToString()); }
	}

	public ConfigFileOpenEncoding PrefsDefaultsFileOpenEncoding {
		get { return (ConfigFileOpenEncoding)GetEnumValueFromSuperset(keyPrefsDefaultsFileOpenEncoding, ConfigFileOpenEncoding.Fixed); }
		set { Set(keyPrefsDefaultsFileOpenEncoding, value.ToString()); }
	}

	/* Uses the same key as PrefsDefaultsFileOpenEncoding but is used when there's a specific encoding set */
	public string PrefsDefaultsFileOpenEncodingFixed {
		get { return GetString(keyPrefsDefaultsFileOpenEncoding, "ISO-8859-15"); }
		set { Set(keyPrefsDefaultsFileOpenEncoding, value); }
	}

	public ConfigFileOpenFallbackEncoding PrefsDefaultsFileOpenFallbackEncoding {
		get { return (ConfigFileOpenFallbackEncoding)GetEnumValueFromSuperset(keyPrefsDefaultsFileOpenFallbackEncoding, ConfigFileOpenFallbackEncoding.Fixed); }
		set { Set(keyPrefsDefaultsFileOpenFallbackEncoding, value.ToString()); }
	}

	/* Uses the same key as PrefsDefaultsFileOpenFallbackEncoding but is used when there's a specific encoding set */
	public string PrefsDefaultsFileOpenFallbackEncodingFixed {
		get { return GetString(keyPrefsDefaultsFileOpenFallbackEncoding, "ISO-8859-15"); }
		set { Set(keyPrefsDefaultsFileOpenFallbackEncoding, value); }
	}

	public ConfigFileSaveEncodingOption PrefsDefaultsFileSaveEncodingOption {
		get { return (ConfigFileSaveEncodingOption)GetEnumValue(keyPrefsDefaultsFileSaveEncodingOption, ConfigFileSaveEncodingOption.KeepExisting); }
		set { Set(keyPrefsDefaultsFileSaveEncodingOption, value.ToString()); }
	}

	public ConfigFileSaveEncoding PrefsDefaultsFileSaveEncoding {
		get { return (ConfigFileSaveEncoding)GetEnumValueFromSuperset(keyPrefsDefaultsFileSaveEncoding, ConfigFileSaveEncoding.Fixed); }
		set { Set(keyPrefsDefaultsFileSaveEncoding, value.ToString()); }
	}

	/* Uses the same key as PrefsDefaultsFileSaveEncoding but is used when there's a specific encoding set */
	public string PrefsDefaultsFileSaveEncodingFixed {
		get { return GetString(keyPrefsDefaultsFileSaveEncoding, "ISO-8859-15"); }
		set { Set(keyPrefsDefaultsFileSaveEncoding, value); }
	}


	
	/* Private members */
	
	private string GetString (string key, string defaultValue) {
		try {
			return (string)client.Get(key);
		}
		catch (Exception e) {
			Console.Error.WriteLine(e);
			return defaultValue;
		}
	}
	
	private bool GetBool (string key, bool defaultValue) {
		try {
			return (bool)client.Get(key);
		}
		catch (Exception e) {
			Console.Error.WriteLine(e);
			return defaultValue;
		}
	}

	/* private int GetInt (string key, int defaultValue) {
		try {
			return (int)client.Get(key);
		}
		catch (Exception) {
			return defaultValue;
		}
	}*/
	
	private int GetInt (string key, int defaultValue, int lowerLimit, bool useLowerLimit, int upperLimit, bool useUpperLimit) {
		try {
			int number = (int)client.Get(key);
			if (useLowerLimit && (number < lowerLimit))
				return defaultValue;
			
			if (useUpperLimit && (number > upperLimit))
				return defaultValue;
			
			return number;
		}
		catch (Exception e) {
			Console.Error.WriteLine(e);
			return defaultValue;
		}
	}
	
	private string[] GetStrings (string key, string[] defaultValue) {
		try {
			string[] strings = client.Get(key) as string[];
			if ((strings.Length == 1) && (strings[0] == String.Empty))
				return new string[0];
			else
				return strings;
		}
		catch (Exception e) {
			Console.Error.WriteLine(e);
			return defaultValue;
		}
	}

	/* Gets an enum value from a field which can hold a value not included in the enum (basically assumes an exception
		can occur). */
	private Enum GetEnumValueFromSuperset (string key, Enum defaultValue) {
		try {
			string stringValue = (string)client.Get(key);
			return (Enum)Enum.Parse(defaultValue.GetType(), stringValue);
		}
		catch (Exception) {
			return defaultValue;
		}
	}

	private Enum GetEnumValue (string key, Enum defaultValue) {
		try {
			string stringValue = (string)client.Get(key);
			return (Enum)Enum.Parse(defaultValue.GetType(), stringValue);
		}
		catch (Exception e) {
			Console.Error.WriteLine(e);
			return defaultValue;
		}
	}

	private void SetStrings (string key, string[] values) {
		if (values.Length == 0) {
			string[] newValues = { String.Empty };
			Set(key, newValues);
		}
		else
			Set(key, values);
	}

	private void Set (string key, object val) {
		try {
			client.Set(key, val);
		}
		catch (Exception e) {
			Console.Error.WriteLine(e);
		}
	}

}

}