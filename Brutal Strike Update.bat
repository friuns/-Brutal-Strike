Echo "Updating the game please wait..."

if not exist "Brutal Strike.exe" (
cd %USERPROFILE%\Desktop
mkdir BrutalStrike
cd BrutalStrike
%SystemRoot%\explorer.exe "%USERPROFILE%\Desktop\BrutalStrike"
)

if not exist ./git/ (
if not exist gitSetup.exe powershell -Command "(New-Object Net.WebClient).DownloadFile('https://github.com/git-for-windows/git/releases/download/v2.30.1.windows.1/PortableGit-2.30.1-32-bit.7z.exe', 'gitSetup.exe')"
if not exist gitSetup.exe %SystemRoot%\System32\curl -L https://github.com/git-for-windows/git/releases/download/v2.30.1.windows.1/PortableGit-2.30.1-32-bit.7z.exe --output gitSetup.exe
if not exist gitSetup.exe curl -L https://github.com/git-for-windows/git/releases/download/v2.30.1.windows.1/PortableGit-2.30.1-32-bit.7z.exe --output gitSetup.exe
if not exist gitSetup.exe %SystemRoot%\system32\certutil.exe -urlcache -split -f https://github.com/git-for-windows/git/releases/download/v2.30.1.windows.1/PortableGit-2.30.1-32-bit.7z.exe gitSetup.exe
)



gitSetup.exe -o ./git -y
  git\bin\git config --global user.email "a@example.com"
  git\bin\git config --global user.name "Your a"
  call git\bin\git\post-install.bat
)

set GIT_SSL_NO_VERIFY=true 
git\bin\git.exe init 
git\bin\git.exe stash --keep-index 
git\bin\git.exe remote add origin https://github.com/friuns/-Brutal-Strike.git

git\bin\git.exe fetch origin  --progress  
TIMEOUT /T 1
git\bin\git.exe checkout -f origin/master  --progress 
TIMEOUT /T 1
git\bin\git.exe checkout master  --progress 
TIMEOUT /T 1
git\bin\git.exe merge origin/master  --progress 


pause