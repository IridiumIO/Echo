<p align="center"><img height="190" alt="echobanner" src="https://github.com/user-attachments/assets/4b1c5eb8-c8a9-40c4-b286-f6f6c91155b7" /></p>


<p align="center"><b>With Echo, you can watch for specific programs or games and have your favorite helper tools or scripts start automatically alongside them.</b></p> 

&nbsp;
&nbsp;
<img width="1300" height="800" alt="Echo1" src="https://github.com/user-attachments/assets/96b53f17-6c2f-4fd6-b41d-2e00b84a5f7f" />

---
Instead of manually opening several programs, creating batch files, or using the Task Scheduler to set up your environment, Echo uses native Windows monitors to detect when you run a specified program, and then automatically launches any number of selected programs/processes/websites/folders/files/etc. 

Most importantly, it can then optionally *close* all those extra programs when the main game / program exits. No need to set everything to run on Windows Startup, they can open/close with your other programs instead. <sup>See caveats</sup> 


## Installation

If you wish to support me, you can download Echo from Steam for the price of a coffee (coming soon)

<p align="center"><a href="https://store.steampowered.com/app/3926000/Echo/"><img src="https://img.shields.io/badge/Download-000000?style=for-the-badge&logo=Steam&logoColor=white&label=Steam" height=50></a>

Otherwise, it is always available for free from the Github releases page.

## Features

- Monitor any programs
  - Automatically finds and lists all Steam Games for easy adding
- Any of the following can be added as a triggered launch for each program:
  - Other Programs
  - Autohotkey scripts
  - Website URLs
  - Folders
  - Any other files will be launched using whatever the filetype default is set to in Windows. e.g. a Word Document will open in MS Word
- You can also specify multiple programs to listen for under each heading:
  - For example, you can specify `PathOfExileSteam.exe` and `PathOfExileSteam_64.exe` and both will fire all the helpers specified under the `Path of Exile` entry
- Config Options
  - Add to Windows Start Menu
  - Run automatically on Windows Startup
<img width="1300" height="800" alt="Echo2" src="https://github.com/user-attachments/assets/673ae38b-6c6c-4a64-977a-fcc28104a2a1" />


## Examples

When running Path of Exile, I always want the following to run:
- Awaked POE Trade
- POE Overlay
- A custom autohotkey script
- Open the POE Trade website

Or, when opening Steam:
- Run Steam Achievement Notifier

Or, when running Visual Studio:
- Open Postman
- Launch custom build monitors

## Caveats
Most programs and games can be detected / launched, but some situations will lead to inaccurate detections. Specifically, when using the `Kill on Close` option, if the triggered process launches sub-processes or redirects to a UWP application, this may not be closed properly on exit. 
Websites and folders, which launch through redirection pathways, also can't be automatically closed on exit. 

This is why an option to run programs in `Hidden` mode is not included; you will always be able to manually close any programs that are run

## Planned (for triggered processes)
- [ ] Run as Admin
- [ ] Delay start
- [ ] Custom arguments
- [ ] Run triggered process on program exit instead of program startup

 -----
 ### Like this project?
 Please consider leaving a tip on Ko-Fi :) 
  
 <p align="center"><a href='https://ko-fi.com/iridiumio' target='_blank'><img height='42' style='border:0px;height:42px;' src='https://cdn.ko-fi.com/cdn/kofi3.png?v=3' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a></p>
  
