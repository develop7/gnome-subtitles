AC_PREREQ(2.58)
AC_INIT(gnome-subtitles, 1.3, http://bugzilla.gnome.org/enter_bug.cgi?product=gnome-subtitles)

GNOME_COMMON_INIT

GNOME_DOC_INIT
GTK_DOC_CHECK([1.0])

AC_CONFIG_MACRO_DIR([m4])
AC_CONFIG_SRCDIR([configure.ac])

AM_INIT_AUTOMAKE([1.9 tar-ustar])
AM_MAINTAINER_MODE

IT_PROG_INTLTOOL([0.35.0])

AC_PROG_LIBTOOL

AC_PROG_INSTALL
AC_PREFIX_PROGRAM(mono)

dnl Gettext stuff.

GETTEXT_PACKAGE=gnome-subtitles
AC_SUBST(GETTEXT_PACKAGE)
AC_DEFINE_UNQUOTED(GETTEXT_PACKAGE,"$GETTEXT_PACKAGE", [Gettext package])

AM_GLIB_GNU_GETTEXT

dnl pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
	AC_MSG_ERROR([You need to install pkg-config])
fi

dnl C# compiler
AC_PATH_PROG(CSC, csc, no)
AC_PATH_PROG(MCS, dmcs, no)
AC_PATH_PROG(MONO, mono, no)

CS="C#"
if test "x$CSC" = "xno" -a "x$MCS" = "xno"  ; then
	dnl AC_MSG_ERROR([You need to install a C# compiler])
	AC_MSG_ERROR([No $CS compiler found])
fi

if test "x$MCS" = "xno" ; then
	MCS=$CSC
fi

if test "x$MONO" = "xno"; then
	AC_MSG_ERROR([No mono runtime found])
fi

dnl create build dir if it does not exist
if test ! -d "build"
then
	mkdir build
fi

dnl pkg-config
MONO_REQUIRED_VERSION=1.1
GTKSHARP_REQUIRED_VERSION=2.12
GTK_REQUIRED_VERSION=2.12

PKG_CHECK_MODULES(gstreamer, [gstreamer-video-1.0 >= 1.0])
PKG_CHECK_MODULES(gstreamer_plugins_base, [gstreamer-plugins-base-1.0 >= 1.0])
AC_SUBST(gstreamer_CFLAGS)
AC_SUBST(gstreamer_LIBS)

PKG_CHECK_MODULES(MONO, mono >= $MONO_REQUIRED_VERSION)
PKG_CHECK_MODULES(GTKSHARP, gtk-sharp-2.0 glade-sharp-2.0 gconf-sharp-2.0 >= $GTKSHARP_REQUIRED_VERSION)
PKG_CHECK_MODULES(GTK, gtk+-2.0 >= $GTK_REQUIRED_VERSION,, [
	PKG_CHECK_MODULES(GTK, gtk+-3.0 >= $GTK_REQUIRED_VERSION)
])

dnl Check gtk-sharp version
PKG_CHECK_MODULES(GTKSHARP_INVALID, gtk-sharp-2.0 >= 2.12.2 gtk-sharp-2.0 <= 2.12.6, gtksharp_invalid=true, gtksharp_invalid=false)

if test "x$gtksharp_invalid" = "xtrue"; then
 	AC_MSG_ERROR([gtk-sharp versions between 2.12.2 and 2.12.6 (inclusive) are not supported.])
fi

PKG_PROG_PKG_CONFIG()

AC_SUBST(MCS)

dnl Handle GConf
AC_PATH_PROG(GCONFTOOL, gconftool-2, no)
AM_GCONF_SOURCE_2

AC_OUTPUT([
Makefile
data/Makefile
data/gnome-subtitles.appdata.xml
data/gnome-subtitles.desktop
src/Makefile
src/GnomeSubtitles/Execution/AssemblyInfo.cs
src/GnomeSubtitles/Execution/gnome-subtitles
help/Makefile
po/Makefile.in])
