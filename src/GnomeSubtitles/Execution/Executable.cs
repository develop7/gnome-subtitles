/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2007 Pedro Castro
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

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GnomeSubtitles {

public class Executable {

	[DllImport("libc")]
	private static extern int prctl(int option, byte [] arg2, ulong arg3, ulong arg4, ulong arg5); //Used in Linux
	[DllImport("libc")]
	private static extern void setproctitle(byte [] fmt, byte [] str_arg); //Used in BSD's

	public static bool SetProcessNamePrctl (string name) {
		try {
			if (prctl(15, Encoding.ASCII.GetBytes(name + "\0"), 0, 0, 0) != 0) { // 15 = PR_SET_NAME
				Console.Error.WriteLine("Error setting process name with prctl: " + Mono.Unix.Native.Stdlib.GetLastError());
			}
		}
		catch (Exception e) {
        	Console.Error.WriteLine("Setting the process name using prctl has thrown an exception:");
        	Console.Error.WriteLine(e);
        	return false;
        }
        return true;
	}
	
	public static bool SetProcessNameSetproctitle (string name) {
		try {
			setproctitle(Encoding.ASCII.GetBytes("%s\0"), Encoding.ASCII.GetBytes(name + "\0"));
		}
		catch (Exception e) {
        	Console.Error.WriteLine("Setting the process name using setproctitle has thrown an exception:");
        	Console.Error.WriteLine(e);
        	return false;
        }
        return true;
	}

    public static void SetProcessName(string name) {
    	if (!SetProcessNamePrctl(name))
    		SetProcessNameSetproctitle(name);
    }

	public static void Main (string[] args) {
		Execution execution = new Execution(args);
		SetProcessName(execution.ExecutableName);
		Global.Run(execution);
	}

}

}