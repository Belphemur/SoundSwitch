#include "idp\idp.iss"
#include "idp\idplang\german.iss"
#include "idp\idplang\french.iss"
#include "idp\idplang\italian.iss"
#include "idp\idplang\spanish.iss"
#include "idp\idplang\brazilianPortuguese.iss"

[CustomMessages]
DependenciesDir=MyProgramDependencies

en.depdownload_msg=The following applications are required before setup can continue:%n%n%1%nDownload and install now?
de.depdownload_msg=Die folgenden Programme werden benötigt bevor das Setup fortfahren kann:%n%n%1%nJetzt downloaden und installieren?
fr.depdownload_msg=Pour fonctionner cette application a besoin de:%n%n%1%nTélécharger et installer maintenant?
es.depdownload_msg=Las siguientes aplicaciones son requeridas para que el instalador pueda continurar:%n%n%1%n¿Descargar e instalarlo ahora?
it.depdownload_msg=Prima che l'installazione possa proseguire sono richieste le seguente applicazioni:%n%n%1%nVuoi scaricarle e installarle?
pt_br.depdownload_msg=As seguintes aplicações são necessárias antes que a instalação possa continuar:%n%n%1%nBaixar e instalar agora?

en.depdownload_memo_title=Download dependencies
de.depdownload_memo_title=Abhängigkeiten downloaden
fr.depdownload_memo_title=Télécharger les dépendences
es.depdownload_memo_title=Desargar dependencias
it.depdownload_memo_title=Download dipendenze
pt_br.depdownload_memo_title=Baixar dependências

en.depinstall_memo_title=Install dependencies
de.depinstall_memo_title=Abhängigkeiten installieren
fr.depinstall_memo_title=Installer les dépendences
es.depinstall_memo_title=Instalar dependencias
it.depinstall_memo_title=Installa dipendenze
pt_br.depinstall_memo_title=Instalar dependências

en.depinstall_title=Installing dependencies
de.depinstall_title=Installiere Abhängigkeiten
fr.depinstall_title=Installation des dépendences
es.depinstall_title=Instalando dependenncias
it.depinstall_title=Installazione dipendenze
pt_br.depinstall_title=Instalando dependências

en.depinstall_description=Please wait while Setup installs dependencies on your computer.
de.depinstall_description=Warten Sie bitte während Abhängigkeiten auf Ihrem Computer installiert wird.
fr.depinstall_description=Veuillez patientez le temps que nous installons les dépendences.
es.depinstall_description=Por favor espera mientras el instalador configura dependencias en tu computador.
it.depinstall_description=Attendi il completamento dell'installazione delle dipendenze.
pt_br.depinstall_description=Por favor aguarde enquanto o Setup instala as dependências no seu computador.

en.depinstall_status=Installing %1...
de.depinstall_status=Installiere %1...
fr.depinstall_status=Installe %1...
es.depinstall_status=Instalando %1...
it.depinstall_status=Installazione di %1...
pt_br.depinstall_status=Instalando %1...

en.depinstall_missing=%1 must be installed before setup can continue. Please install %1 and run Setup again.
de.depinstall_missing=%1 muss installiert werden bevor das Setup fortfahren kann. Bitte installieren Sie %1 und starten Sie das Setup erneut.
fr.depinstall_missing=%1 doit être installé pour que l'installation continue. Installez %1 et relancer l'installateur.
es.depinstall_missing=%1 debe ser instalado antes para poder continuar. Por favor, instala %1 y ejecuta Instalador otra vez.
it.depinstall_missing=Prima che l'installazione possa continuare deve essere installato %1.%nInstalla %1 e quindi riesegui l'installazione.
pt_br.depinstall_missing=%1 deve ser instalado para que o setup possa continuar. Por favor instale %1 e execute o instalador novamente.

en.depinstall_error=An error occured while installing the dependencies. Please restart the computer and run the setup again or install the following dependencies manually:%n
de.depinstall_error=Ein Fehler ist während der Installation der Abghängigkeiten aufgetreten. Bitte starten Sie den Computer neu und führen Sie das Setup erneut aus oder installieren Sie die folgenden Abhängigkeiten per Hand:%n
fr.depinstall_error=Nous n'avons pu installer les dépendences. Veuillez redémarrer votre ordinateur et relancer l'installation ou installer ces dépendences manuellement:%n
es.depinstall_error=Un error ocurrió instalando las dependencias. Por favor, reinicia el computador y ejecuta el instaldor otra vez, o instala la siguientes dependencias manualmente:%n
it.depinstall_error=Si è verificato un errore durante l'installazione delle dipendenze.%nRiavvia il computer e esegui nuovamente l'installazione o installa manualmente le seguenti dipendenze:%n
pt_br.depinstall_error=Um erro ocorreu durante a instalação das dependências. Por favor reinicie o computador e execute o instalador novamente ou instale as seguintes dependências manualmente:%n


[Code]
type
	TProduct = record
		File: String;
		Title: String;
		Parameters: String;
		InstallClean : boolean;
		MustRebootAfter : boolean;
	end;

	InstallResult = (InstallSuccessful, InstallRebootRequired, InstallError);

var
	installMemo, downloadMemo, downloadMessage: string;
	products: array of TProduct;
	delayedReboot: boolean;
	DependencyPage: TOutputProgressWizardPage;


procedure AddProduct(FileName, Parameters, Title, Size, URL: string; InstallClean : boolean; MustRebootAfter : boolean);
var
	path: string;
	i: Integer;
begin
	installMemo := installMemo + '%1' + Title + #13;

	path := ExpandConstant('{src}{\}') + CustomMessage('DependenciesDir') + '\' + FileName;
	if not FileExists(path) then begin
		path := ExpandConstant('{tmp}{\}') + FileName;

		idpAddFile(URL, path);

		downloadMemo := downloadMemo + '%1' + Title + #13;
		downloadMessage := downloadMessage + '	' + Title + ' (' + Size + ')' + #13;
	end;

	i := GetArrayLength(products);
	SetArrayLength(products, i + 1);
	products[i].File := path;
	products[i].Title := Title;
	products[i].Parameters := Parameters;
	products[i].InstallClean := InstallClean;
	products[i].MustRebootAfter := MustRebootAfter;
end;

function SmartExec(prod : TProduct; var ResultCode : Integer) : boolean;
begin
	if (LowerCase(Copy(prod.File,Length(prod.File)-2,3)) = 'exe') then begin
		Result := Exec(prod.File, prod.Parameters, '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode);
	end else begin
		Result := ShellExec('', prod.File, prod.Parameters, '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode);
	end;
end;

function PendingReboot : boolean;
var	names: String;
begin
	if (RegQueryMultiStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager', 'PendingFileRenameOperations', names)) then begin
		Result := true;
	end else if ((RegQueryMultiStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager', 'SetupExecute', names)) and (names <> ''))  then begin
		Result := true;
	end else begin
		Result := false;
	end;
end;

function InstallProducts: InstallResult;
var
	ResultCode, i, productCount, finishCount: Integer;
begin
	Result := InstallSuccessful;
	productCount := GetArrayLength(products);

	if productCount > 0 then begin
		DependencyPage := CreateOutputProgressPage(CustomMessage('depinstall_title'), CustomMessage('depinstall_description'));
		DependencyPage.Show;

		for i := 0 to productCount - 1 do begin
			if (products[i].InstallClean and (delayedReboot or PendingReboot())) then begin
				Result := InstallRebootRequired;
				break;
			end;

			DependencyPage.SetText(FmtMessage(CustomMessage('depinstall_status'), [products[i].Title]), '');
			DependencyPage.SetProgress(i, productCount);

			if SmartExec(products[i], ResultCode) then begin
				//setup executed; ResultCode contains the exit code
				//MsgBox(products[i].Title + ' install executed. Result Code: ' + IntToStr(ResultCode), mbInformation, MB_OK);
				if (products[i].MustRebootAfter) then begin
					//delay reboot after install if we installed the last dependency anyways
					if (i = productCount - 1) then begin
						delayedReboot := true;
					end else begin
						Result := InstallRebootRequired;
					end;
					break;
				end else if (ResultCode = 0) then begin
					finishCount := finishCount + 1;
				end else if (ResultCode = 3010) then begin
					//ResultCode 3010: A restart is required to complete the installation. This message indicates success.
					delayedReboot := true;
					finishCount := finishCount + 1;
				end else begin
					Result := InstallError;
					break;
				end;
			end else begin
				//MsgBox(products[i].Title + ' install failed. Result Code: ' + IntToStr(ResultCode), mbInformation, MB_OK);
				Result := InstallError;
				break;
			end;
		end;

		//only leave not installed products for error message
		for i := 0 to productCount - finishCount - 1 do begin
			products[i] := products[i+finishCount];
		end;
		SetArrayLength(products, productCount - finishCount);

		DependencyPage.Hide;
	end;
end;

function PrepareToInstall(var NeedsRestart: boolean): String;
var
	i: Integer;
	s: string;
begin
	delayedReboot := false;

	case InstallProducts() of
		InstallError: begin
			s := CustomMessage('depinstall_error');

			for i := 0 to GetArrayLength(products) - 1 do begin
				s := s + #13 + '	' + products[i].Title;
			end;

			Result := s;
			end;
		InstallRebootRequired: begin
			Result := products[0].Title;
			NeedsRestart := true;

			//write into the registry that the installer needs to be executed again after restart
			RegWriteStringValue(HKEY_CURRENT_USER, 'SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce', 'InstallBootstrap', ExpandConstant('{srcexe}'));
			end;
	end;
end;

function NeedRestart : boolean;
begin
	if (delayedReboot) then
		Result := true;
end;

function UpdateReadyMemo(Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo, MemoComponentsInfo, MemoGroupInfo, MemoTasksInfo: String): String;
var
	s: string;
begin
	if downloadMemo <> '' then
		s := s + CustomMessage('depdownload_memo_title') + ':' + NewLine + FmtMessage(downloadMemo, [Space]) + NewLine;
	if installMemo <> '' then
		s := s + CustomMessage('depinstall_memo_title') + ':' + NewLine + FmtMessage(installMemo, [Space]) + NewLine;

	s := s + MemoDirInfo + NewLine + NewLine + MemoGroupInfo

	if MemoTasksInfo <> '' then
		s := s + NewLine + NewLine + MemoTasksInfo;

	Result := s
end;

function NextButtonClick(CurPageID: Integer): boolean;
begin
	Result := true;

	if CurPageID = wpReady then begin
		if downloadMemo <> '' then begin
			//change isxdl language only if it is not english because isxdl default language is already english
			//if (ActiveLanguage() <> 'en') then begin
			//	ExtractTemporaryFile(CustomMessage('isxdl_langfile'));
			//	isxdl_SetOption('language', ExpandConstant('{tmp}{\}') + CustomMessage('isxdl_langfile'));
			//end;
			//isxdl_SetOption('title', FmtMessage(SetupMessage(msgSetupWindowTitle), [CustomMessage('appname')]));
      idpSetOption('detailedmode', 'true');

			if SuppressibleMsgBox(FmtMessage(CustomMessage('depdownload_msg'), [downloadMessage]), mbConfirmation, MB_YESNO, IDYES) = IDNO then
				Result := false
			else begin
        Result := true;
      end;
     end;
  end;
end;

function IsX86: boolean;
begin
	Result := (ProcessorArchitecture = paX86) or (ProcessorArchitecture = paUnknown);
end;

function IsX64: boolean;
begin
	Result := Is64BitInstallMode and (ProcessorArchitecture = paX64);
end;

function IsIA64: boolean;
begin
	Result := Is64BitInstallMode and (ProcessorArchitecture = paIA64);
end;

function GetString(x86, x64, ia64: String): String;
begin
	if IsX64() and (x64 <> '') then begin
		Result := x64;
	end else if IsIA64() and (ia64 <> '') then begin
		Result := ia64;
	end else begin
		Result := x86;
	end;
end;

function GetArchitectureString(): String;
begin
	if IsX64() then begin
		Result := '_x64';
	end else if IsIA64() then begin
		Result := '_ia64';
	end else begin
		Result := '';
	end;
end;
