# SIYA'S CYBER CHATBOT — Part 3 POE
## PROG6221/W — Programming 2A

---

## SETUP INSTRUCTIONS (Step by Step)

### Step 1 — Install SQLite NuGet Package
1. Open your project in Visual Studio
2. Go to: **Tools → NuGet Package Manager → Manage NuGet Packages for Solution**
3. Click the **Browse** tab
4. Search: `System.Data.SQLite`
5. Select it → click **Install**
6. Click **OK** on any popup

### Step 2 — Add New Class Files
Right-click your project in Solution Explorer → **Add → New Item → Class**
Add these files (paste the code from each file provided):

| File | Purpose |
|------|---------|
| `TaskItem.cs` | Data model for a task |
| `DatabaseManager.cs` | SQLite CRUD operations |
| `ActivityLogger.cs` | Records chatbot actions |
| `QuizManager.cs` | Quiz questions and scoring |
| `NLPProcessor.cs` | Natural language intent detection |

### Step 3 — Replace Existing Files
Replace the content of these existing files:
- `Chatbot.cs` → with the new enhanced version
- `MainWindow.xaml` → with the new version (has sidebar)
- `MainWindow.xaml.cs` → with the new version

### Step 4 — Keep These Files Unchanged
- `User.cs` — unchanged from Part 2 (or use the new copy provided)
- `App.xaml` and `App.xaml.cs` — do NOT touch these

### Step 5 — Build and Run
Press **F5** or **Ctrl+F5** to run.
The SQLite database file (`cybertasks.db`) is created automatically next to the .exe.

---

## HOW TO USE

### Task Assistant (Task 1 — 30 marks)
| Command | What it does |
|---------|-------------|
| `add task Enable two-factor authentication` | Adds a new task |
| `add a task to review my privacy settings` | Natural language task add |
| `remind me to update my password` | Also adds a task |
| `show tasks` | Lists all tasks |
| `complete task 1` | Marks task ID 1 as done |
| `delete task 2` | Deletes task ID 2 |
| Click **➕ Add Task** sidebar button | Prompts you to type the task name |

### Quiz (Task 2 — 15 marks)
| Command | What it does |
|---------|-------------|
| `start quiz` | Begins the 12-question quiz |
| Click **🎮 Start Quiz** | Same as above |
| Type `A`, `B`, `C`, `D`, `True`, or `False` | Answers a question |

### Activity Log (Task 4 — 10 marks)
| Command | What it does |
|---------|-------------|
| `show activity log` | Shows last 10 actions |
| `what have you done` | Same as above |
| `show full log` | Shows all recorded actions |
| Click **📋 Activity Log** | Same as above |

### NLP (Task 3 — 10 marks)
The chatbot understands natural variations:
- "Can you remind me to update my password?" → creates a task
- "I need to set up two factor auth" → creates a task
- "set a reminder for checking privacy settings" → creates a reminder
- "quiz me" / "test me" / "trivia" → starts the quiz

---

## FEATURES FROM PARTS 1 & 2 (still present)

- ✅ ASCII art header
- ✅ Voice greeting (WAV file)
- ✅ Keyword recognition (phishing, passwords, privacy, scams, viruses, browsing)
- ✅ Random tips (Dictionary<string, string[]>)
- ✅ "Tell me more" conversation flow
- ✅ Memory / favourite topic
- ✅ Sentiment detection (worried, curious, frustrated, happy, tired)
- ✅ Spinner animation + typing effect
- ✅ Quick topic buttons

---

## DATABASE

The SQLite database file `cybertasks.db` is created automatically in the same folder as the running executable. It contains one table:

```sql
CREATE TABLE Tasks (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Title       TEXT    NOT NULL,
    Description TEXT    NOT NULL,
    Reminder    TEXT    DEFAULT '',
    IsCompleted INTEGER DEFAULT 0,
    CreatedAt   TEXT    NOT NULL
);
```

CRUD operations covered (for full database marks):
- **CREATE** — `AddTask()`
- **READ**   — `GetAllTasks()`
- **UPDATE** — `MarkTaskComplete()`, `UpdateReminder()`
- **DELETE** — `DeleteTask()`

---

## PROJECT STRUCTURE

```
CyberSecurityChatbotGUI/
├── App.xaml                  (unchanged)
├── App.xaml.cs               (unchanged)
├── MainWindow.xaml           (REPLACED — sidebar added)
├── MainWindow.xaml.cs        (REPLACED — sidebar handlers added)
├── User.cs                   (unchanged)
├── Chatbot.cs                (REPLACED — NLP/tasks/quiz/log integrated)
├── TaskItem.cs               (NEW — Part 3)
├── DatabaseManager.cs        (NEW — Part 3, SQLite CRUD)
├── ActivityLogger.cs         (NEW — Part 3)
├── QuizManager.cs            (NEW — Part 3, 12 questions)
├── NLPProcessor.cs           (NEW — Part 3)
└── Chatbot.wav               (Part 1, unchanged)
```

---

## GITHUB COMMITS GUIDE (for GitHub marks)

Commit your code with meaningful messages. Suggested commit history:
1. `Initial Part 3 setup: add TaskItem and DatabaseManager with SQLite`
2. `Add QuizManager with 12 cybersecurity questions (mixed types)`
3. `Add NLPProcessor for flexible intent detection`
4. `Add ActivityLogger for session action tracking`
5. `Enhance Chatbot.cs: integrate tasks, quiz, NLP, and activity log`
6. `Update MainWindow: add sidebar with task/quiz/log buttons`

Create a GitHub release tagged `v3.0` with release notes.

---

*PROG6221/W — Programming 2A | POE Part 3*
