// SoundSwitch installer language configuration
// Copyright © 2010-2025 SoundSwitch

#ifndef languagesIss
#define languagesIss

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"; LicenseFile: "..\LICENSE.txt"
Name: "de"; MessagesFile: "compiler:Languages\German.isl"; LicenseFile: "..\LICENSE.txt"
Name: "fr"; MessagesFile: "compiler:Languages\French.isl"; LicenseFile: "..\LICENSE.txt"
Name: "es"; MessagesFile: "compiler:Languages\Spanish.isl"; LicenseFile: "..\LICENSE.txt"
Name: "it"; MessagesFile: "compiler:Languages\Italian.isl"; LicenseFile: "..\LICENSE.txt"
Name: "pl"; MessagesFile: "compiler:Languages\Polish.isl"; LicenseFile: "..\LICENSE.txt"
Name: "pt_br"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"; LicenseFile: "..\LICENSE.txt"
Name: "nl"; MessagesFile: "compiler:Languages\Dutch.isl"; LicenseFile: "..\LICENSE.txt"
Name: "ru"; MessagesFile: "compiler:Languages\Russian.isl"; LicenseFile: "..\LICENSE.txt"
Name: "ko"; MessagesFile: "Languages\Korean.isl"; LicenseFile: "..\LICENSE.txt"
Name: "zh"; MessagesFile: "Languages\ChineseSimplified.isl"; LicenseFile: "..\LICENSE.txt"

; Include custom messages for each language
#include "..\Languages\CustomMessages.en.iss"
#include "..\Languages\CustomMessages.de.iss"
#include "..\Languages\CustomMessages.fr.iss"
#include "..\Languages\CustomMessages.es.iss"
#include "..\Languages\CustomMessages.it.iss"
#include "..\Languages\CustomMessages.pl_pl.iss"
#include "..\Languages\CustomMessages.pt_br.iss"
#include "..\Languages\CustomMessages.nl.iss"
#include "..\Languages\CustomMessages.ru_ru.iss"
#include "..\Languages\CustomMessages.ko.iss"
#include "..\Languages\CustomMessages.zh.iss"

#endif