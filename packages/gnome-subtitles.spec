##
# spec file for package gnome-subtitles
#
# Copyright (c) Damien Carbery <daymobrew users.sourceforge.net>
#               Henrique Malheiro <henrique.malheiro gmail.com>
#               Pedro Castro <noup users.sourceforge.net>
#
#
%define dist	%(test -f /etc/redhat-release && echo .fc`rpm -qf --qf='%{VERSION}' /etc/redhat-release`)
%define OnSuSE   %(test -f /etc/SuSE-release && echo 1 || echo 0)

Name:           gnome-subtitles
Summary:        Movie subtitling for the Gnome desktop
Version:        0.1
Release:        1%dist
%if %OnSuSE
Distribution:   SuSE 10.1
%endif
Group:          Applications/Multimedia
License:        GPL
URL:            http://gnome-subtitles.sourceforge.net/
Source:         %{name}-%{version}.tar.gz
BuildRoot:      %{_tmppath}/%{name}-%{version}-build
BuildArch:      noarch

Prereq:         /sbin/ldconfig

Requires:       mono-core >= 1.0
Requires:       gtk2 >= 2.8
Requires:       gtk-sharp2 >= 2.8

%if %OnSuSE
Requires:       glade-sharp2 >= 2.8
Requires:       gnome-sharp2 >= 2.8
%endif

BuildRequires:  mono-devel >= 1.0
BuildRequires:  gtk2-devel >= 2.8
BuildRequires:  gtk-sharp2 >= 2.8

%if %OnSuSE
BuildRequires:  glade-sharp2 >= 2.8
BuildRequires:  gnome-sharp2 >= 2.8
%endif

%if "%fedora" >= "6"
BuildRequires:	gtk-sharp2-devel >= 2.10
BuildRequires:	gnome-sharp-devel >= 2.16
%endif

%description
Gnome Subtitles is a subtitle editor for the GNOME desktop. It supports the most
common subtitle formats and allows for subtitle editing, conversion and
synchronization.

%prep
%setup -q

%build
./configure --prefix=/usr --bindir=/usr/bin --libdir=/usr/lib --datadir=/usr/share
make

%install
make DESTDIR=$RPM_BUILD_ROOT install

%clean
rm -rf $RPM_BUILD_ROOT

%post
/sbin/ldconfig

%postun
/sbin/ldconfig

%files
%defattr(-,root,root)
%{_bindir}/%{name}
%{_libdir}/%{name}
%{_datadir}/applications/%{name}.desktop
%{_datadir}/pixmaps/%{name}.png
%doc README NEWS AUTHORS COPYING CREDITS TODO


%changelog
* Thu Dec 14 2006 - Henrique Malheiro <henrique.malheiro@gmail.com>
- Updated the application icon extension from svg to png.
- Updated the build requirements for fedora core 6 to include gtk-sharp2-devel
  and gnome-sharp-devel.
- Added the dist tag for using the same spec file for both distributions, fedora
  core 5 and fedora core 6 and removed the distribution tag for fedora. This
  will be useful for Fedora Core Extras.
* Wed Dec 13 2006 - Pedro Castro <noup users.sourceforge.net>
- Updated the website URL.
- Updated for release 0.1.
* Tue Oct 31 2006 - Damien Carbery <daymobrew users.sourceforge.net>
- Merged with the SUSE Linux spec file.
* Tue Oct 31 2006 - Henrique Malheiro <henrique.malheiro gmail.com>
- Adapted spec file for the new 0.0.3 version of gnome-subtitles.
- Modified build architecture to noarch. Configure macro had to be replaced by
  a custom configure.
- Corrected the path to the executable in the files section.
- Added new doc files to the files section.
* Sun Oct 22 2006 - Henrique Malheiro <henrique.malheiro gmail.com>
- Initial version of the spec file, written for the 0.0.2 version of gnome-subtitles.