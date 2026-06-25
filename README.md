# Siya's Cyber Security Chatbot — Part 3 POE

A **Windows Presentation Foundation (WPF)** cybersecurity awareness chatbot built in C# (.NET Framework). Part 3 extends the existing chatbot from Parts 1 and 2 by adding a task assistant, a cybersecurity quiz, smarter natural language understanding, and an activity log — all integrated into the same dark-themed GUI.

---

## Preview

```
╔════════════════════════════════════════╗
║     SIYA'S CYBER CHATBOT  — PART 3    ║
║     Stay Safe Online! 🛡️               ║
╚════════════════════════════════════════╝
```

---

## What's New in Part 3

###  Task Assistant (with SQLite Database)
Users can manage cybersecurity-related tasks directly in the chat. Every task is saved to a local SQLite database so nothing is lost between sessions.

- Add tasks using natural language — `"add task Enable 2FA"` or `"remind me to update my password"`
- View all tasks with their status, description, and reminder
- Mark a task as complete or delete it by ID
- Set optional reminders — `"Yes, remind me in 3 days"` or `"remind me tomorrow"`
- The database file (`cybertasks.db`) is created automatically — no setup needed

###  Cybersecurity Quiz
An interactive quiz that tests the user's knowledge on cybersecurity topics. Questions are shuffled every session so it feels fresh each time.

- 12 questions covering phishing, passwords, safe browsing, ransomware, and social engineering
- Mix of **multiple-choice** (A/B/C/D) and **true/false** question types
- Immediate feedback after every answer with a brief explanation
- Final score with a grade at the end — from "Keep learning!" to "Cybersecurity Champion!"

###  NLP Simulation
The chatbot now understands the same command phrased in many different ways, instead of requiring exact keywords.

| User types | What the bot understands |
|---|---|
| `"remind me to update my password"` | Add a task |
| `"I need to back up my files"` | Add a task |
| `"quiz me"` / `"test me"` / `"trivia"` | Start the quiz |
| `"what have you done for me?"` | Show activity log |
| `"my tasks"` / `"list tasks"` | View all tasks |

###  Activity Log
The chatbot records every significant action it takes during the session, with timestamps. The user can ask to see this log at any time.

- Logs task additions, completions, deletions, reminders set, quiz results, and topics discussed
- Shows the **last 10 entries** by default to keep it readable
- Type `"show full log"` to see the entire session history

---

## Sidebar Navigation

A sidebar was added to the left of the chat window for quick access to all Part 3 features without needing to type commands:

- ** Add Task** — pre-fills the input so you just type the task name
- ** View Tasks** — shows all tasks from the database
- ** Complete Task** — pre-fills the input with the task ID prompt
- ** Delete Task** — same for deleting
- ** Start Quiz** — launches the quiz immediately
- ** Activity Log** — shows the last 10 actions
- ** Full Log** — shows the entire session log

---

## Everything from Parts 1 & 2 Still Works

All features from the previous parts are fully preserved:

- ASCII art header and voice greeting on startup
- Keyword recognition for phishing, passwords, privacy, scams, viruses, links, and browsing
- 5 random tip variations per topic using `Dictionary<string, string[]>`
- `"Tell me more"` conversation flow to continue on the last topic
- Memory system — remembers your favourite topic and shows it in the header
- Sentiment detection for worried, curious, frustrated, happy, and tired emotions
- Spinning thinking indicator and character-by-character typing animation
- Quick-access topic buttons along the bottom

---

## Project Structure

```
CyberSecurityChatbotGUI/
│
├── User.cs                 # Stores user name, question count, favourite topic (unchanged)
├── Chatbot.cs              # All response logic — now includes task, quiz, NLP, and log routing
├── TaskItem.cs             # Data model for a single cybersecurity task
├── DatabaseManager.cs      # SQLite — add, read, update, and delete tasks
├── QuizManager.cs          # 12 quiz questions, answer checking, and scoring
├── NLPProcessor.cs         # Intent detection from natural language input
├── ActivityLogger.cs       # Records chatbot actions with timestamps
├── MainWindow.xaml         # GUI layout — now includes the left sidebar
├── MainWindow.xaml.cs      # Animations, timers, sidebar and input handlers
└── Chatbot.wav             # Voice greeting played on startup
```

---

## How to Use

| What you type | What happens |
|---|---|
| `add task Enable 2FA` | Adds a task to the database |
| `show tasks` | Lists all your saved tasks |
| `complete task 1` | Marks task 1 as done |
| `delete task 2` | Removes task 2 permanently |
| `start quiz` | Begins the 12-question quiz |
| `show activity log` | Shows the last 10 chatbot actions |
| `tell me more` | Gets another tip on the last topic |
| `menu` | Lists all available commands |
| `exit` | Ends the session with a summary |

---

## Technical Highlights

- **SQLite via `System.Data.SQLite`** — local database, no server needed, file created automatically
- **NLP with synonym lists and Regex** — maps varied phrases to clean intents without external libraries
- **`Dictionary<string, string[]>`** — random tips per topic, expandable with no logic changes
- **`DispatcherTimer`** — drives the spinner (every 200ms) and typing effect (every 18ms)
- **`async`/`await`** — 2.5-second thinking delay that keeps the UI fully responsive
- **OOP design** — each class has one responsibility, making the code clean and easy to extend

---

## 🇿🇦 South African Context

- Reports scams to **SABRIC**: `0861 022 339`
- Refers cybercrime to **SAPS Crime Stop**: `0860 010 111`
- National cybersecurity email: `report@cybersecurity.gov.za`

---

## Author

**Siyabonga** — [@Siyabonga95-lab](https://github.com/Siyabonga95-lab)

> *"Knowledge is your best defence online."*
