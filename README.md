# Rudal Sekeloa - Reloaded 💀
A repository contains 4 Bots (1 main and 3 alternatives) in `C# (.net)` using `Java` game engine that implement various Greedy algorithms in [**Tank Royale Robocode**](https://robocode-dev.github.io/tank-royale/) with the goal of winning the game. 

---

<!-- CONTRIBUTOR -->
<div align="center" id="contributor">
  <strong>
    <h3>~ Rudal Sekeloa Reloaded 🚀 ~</h3>
    <table align="center">
      <tr align="center">
        <td>NIM</td>
        <td>Nama</td>
        <td>GitHub</td>
      </tr>
      <tr align="center">
        <td>13523004</td>
        <td>Razi Rachman Widyadhana</td>
        <td><a href="https://github.com/zirachw">@zirachw</a></td>
      </tr>
      <tr align="center">
        <td>13523090</td>
        <td>Nayaka Ghana Subrata</td>
        <td><a href="https://github.com/Nayekah">@Nayekah</a></td>
      </tr>
      <tr align="center">
        <td>13523098</td>
        <td>Muhammad Adha Ridwan</td>
        <td><a href="https://github.com/adharidwan">@adharidwan</a></td>
      </tr>
    </table>
  </strong>
</div>

<div align="center">
  <h3 align="center">~ Tech Stacks ~ </h3>

  <p align="center">

[![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white)][Csharp-url]
[![Java](https://img.shields.io/badge/java-%23ED8B00.svg?style=for-the-badge&logo=openjdk&logoColor=white)][Java-url]
[![Gradle](https://img.shields.io/badge/Gradle-02303A.svg?style=for-the-badge&logo=Gradle&logoColor=white)][Gradle-url]
  
  </p>
</div>

---

## 📦 Installation & Setup

### Requirements
- Git
- Any IDE (recommended: VSCode)
- dotnet version 8.0
- Java

### Installing dependencies

#### On Windows, do this
1. Git
      ```bash
   https://git-scm.com/download/win
   ```
2. VSCode
      ```bash
   winget install microsoft.visualstudiocode
   ```
3. dotnet
      ```bash
   https://dotnet.microsoft.com/en-us/download
   ```
4. Java

---
## How To Run

### **Windows**
### Bots
1. Open a terminal
2. Clone the repository (if not already cloned)
      ```bash
   git clone https://github.com/zirachw/Tubes1_Rudal-Sekeloa-Reloaded.git
   ```
3. Make Tubes1_Rudal-Sekeloa-Reloaded as root directory:
      ```bash
   cd Tubes1_Rudal-Sekeloa-Reloaded
   ```
5. Direct it to bot's root folder:
   ```bash
   cd src/alternative-bots/[botname]
   ```
   if you want to compile alternative bots, or
      ```bash
   cd src/main-bots/[botname]
   ```
   if you want to compile main bot
4. Compile the bot (bin obj making process):
   ```bash
   [botname].cmd
   ```
   
### Game Engine
(See the bots section first for the prerequisite)

1. Open a terminal
2. Clone the repository (if not already cloned)
      ```bash
   git clone https://github.com/zirachw/Tubes1_Rudal-Sekeloa-Reloaded.git
   ```
3. Make Tubes1_Rudal-Sekeloa-Reloaded as root directory:
      ```bash
   cd Tubes1_Rudal-Sekeloa-Reloaded
   ```
4. Run the following command to start the Game Engine
   ```bash
   java -jar robocode-tankroyale-gui-0.30.0.jar
   ```
---
### **Linux (UNIX system)**
### Bots
1. Open a terminal
2. Clone the repository (if not already cloned)
      ```bash
   git clone https://github.com/zirachw/Tubes1_Rudal-Sekeloa-Reloaded.git
   ```
3. Make Tubes1_Rudal-Sekeloa-Reloaded as root directory:
      ```bash
   cd Tubes1_Rudal-Sekeloa-Reloaded
   ```
4. Direct it to bot's root folder:
   ```bash
   cd src/alternative-bots/[botname]
   ```
   if you want to compile alternative bots, or
      ```bash
   cd src/main-bots/[botname]
   ```
   if you want to compile main bot
4. Compile the bot (bin obj making process):
   ```bash
   ./[botname].sh
   ```
   
### Game Engine
(See the bots section first for the prerequisite)

1. Open a terminal
2. Clone the repository (if not already cloned)
      ```bash
   git clone https://github.com/zirachw/Tubes1_Rudal-Sekeloa-Reloaded.git
   ```
3. Make Tubes1_Rudal-Sekeloa-Reloaded as root directory:
      ```bash
   cd Tubes1_Rudal-Sekeloa-Reloaded
   ```
4. Run the following command to start the Game Engine
   ```bash
   java -jar robocode-tankroyale-gui-0.30.0.jar
   ```
---
## 📱 Repository Structure
```
📂 Tubes1_Rudal-Sekeloa-Reloaded/
├── 📂 docs/
│ ├── .gitkeep
│ └── Rudal Sekeloa Reloaded.pdf
├── 📂 src/
│ ├── 📂 alternative-bots/
│ │ ├── 📂 Kaze/
│ │ │ ├── Kaze.cmd
│ │ │ ├── Kaze.cs
│ │ │ ├── Kaze.csproj
│ │ │ ├── Kaze.json
│ │ │ └── Kaze.sh
│ │ ├── 📂 Sweepredict/
│ │ │ ├── Sweepredict.cmd
│ │ │ ├── Sweepredict.cs
│ │ │ ├── Sweepredict.csproj
│ │ │ ├── Sweepredict.json
│ │ │ └── Sweepredict.sh
│ │ ├── 📂 Waves/
│ │ │ ├── Waves.cmd
│ │ │ ├── Waves.cs
│ │ │ ├── Waves.csproj
│ │ │ ├── Waves.json
│ │ │ └── Waves.sh
│ │ └── .gikeep
│ ├── 📂 main-bots/
│ │ ├── 📂 RudalSekeloa/
│ │ │ ├── RudalSekeloa.cmd
│ │ │ ├── RudalSekeloa.cs
│ │ │ ├── RudalSekeloa.csproj
│ │ │ ├── RudalSekeloa.json
│ │ │ └── RudalSekeloa.sh
│ │ └── .gikeep
├── robocode-tankroyale-gui-0.30.0.jar
└── README.md
```

---
## 📃 Miscellaneous
| No | Points | Yes | No |
| --- | --- | --- | --- |
| 1 | The bot can be run on the Engine that the assistant has modified. | ✔️ | |
| 2 | Created 4 greedy solutions with different heuristics. | ✔️ | |
| 3 | Make reports according to specifications. | ✔️ | |
| 4 | Created a bonus video and uploaded it on Youtube. | ✔️ | |

<br/>
<br/>
<br/>
<br/>

---
<!-- MARKDOWN LINKS & IMAGES -->
[Csharp-url]: https://learn.microsoft.com/en-us/dotnet/csharp/
[Java-url]: https://www.java.com/en/
[Gradle-url]: https://gradle.org/
