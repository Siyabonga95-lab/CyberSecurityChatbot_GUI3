using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CyberSecurityChatbot_GUI3
{
    // ============================================================
    //
    //  I Simulates Natural Language Processing using:
    //   1. Synonym dictionaries — maps many phrasings to one intent
    //   2. Regex patterns       — flexible word-order matching
    //   3. Intent classification — returns a clean Intent enum
    //
    // This allows the  chatbot to understand inputs like:
    //   "Can you remind me to update my password?"
    //   "set a reminder for enabling 2FA"
    //   "I need to set up two factor auth"
    //   ... all mapped to the same TASK_ADD intent.
    //
    // INTENTS DETECTED:
    //   TASK_ADD       — user wants to add a cybersecurity task
    //   TASK_VIEW      — user wants to see their task list
    //   TASK_COMPLETE  — user wants to mark a task done
    //   TASK_DELETE    — user wants to delete a task
    //   REMINDER_SET   — user wants a reminder on the last task
    //   QUIZ_START     — user wants to play the quiz
    //   ACTIVITY_LOG   — user wants to see the activity log
    //   TOPIC_*        — cybersecurity topic questions
    //   UNKNOWN        — falls back to existing Chatbot.GetResponse()
    // ============================================================

    public enum Intent
    {
        TASK_ADD,
        TASK_VIEW,
        TASK_COMPLETE,
        TASK_DELETE,
        REMINDER_SET,
        QUIZ_START,
        QUIZ_ANSWER,
        ACTIVITY_LOG,
        ACTIVITY_LOG_FULL,
        SMALL_TALK,
        UNKNOWN
    }

    public class NLPProcessor
    {
        // ── SYNONYM / TRIGGER MAPS ───────────────────────────────
        // Each list contains all the ways a user might express the intent.

        private static readonly string[] TaskAddTriggers =
        {
            "add task", "add a task", "create task", "new task",
            "remind me to", "can you remind me", "set a task",
            "i need to", "i want to", "make a note", "note that",
            "set up task", "schedule task", "task to"
        };

        private static readonly string[] TaskViewTriggers =
        {
            "show tasks", "view tasks", "list tasks", "my tasks",
            "what are my tasks", "show my tasks", "all tasks",
            "display tasks", "see tasks", "check tasks"
        };

        private static readonly string[] TaskCompleteTriggers =
        {
            "complete task", "mark task", "mark done", "finished task",
            "done with task", "task done", "completed", "mark complete",
            "mark as done", "mark as complete", "i finished", "tick off"
        };

        private static readonly string[] TaskDeleteTriggers =
        {
            "delete task", "remove task", "cancel task",
            "get rid of task", "erase task", "drop task"
        };

        private static readonly string[] ReminderTriggers =
        {
            "remind me in", "set reminder", "reminder in", "remind me",
            "set a reminder", "add reminder", "yes remind me",
            "yes, remind", "remind in", "set reminder for"
        };

        private static readonly string[] QuizTriggers =
        {
            "start quiz", "play quiz", "quiz me", "take quiz",
            "begin quiz", "test me", "quiz time", "start the quiz",
            "play the quiz", "let's play", "lets play", "trivia"
        };

        private static readonly string[] LogTriggers =
        {
            "show activity log", "activity log", "what have you done",
            "show log", "view log", "recent actions", "history",
            "what did you do", "show history", "action log"
        };

        private static readonly string[] LogFullTriggers =
        {
            "show full log", "full log", "all actions", "full history",
            "show all actions", "complete log"
        };

        // ── DETECT INTENT ────────────────────────────────────────
        // Main method — returns the detected Intent for a given message.
        public Intent DetectIntent(string input)
        {
            string msg = input.ToLower().Trim();

            // Order matters: check specific intents before general ones

            if (MatchesAny(msg, LogFullTriggers)) return Intent.ACTIVITY_LOG_FULL;
            if (MatchesAny(msg, LogTriggers)) return Intent.ACTIVITY_LOG;
            if (MatchesAny(msg, QuizTriggers)) return Intent.QUIZ_START;
            if (MatchesAny(msg, TaskCompleteTriggers)) return Intent.TASK_COMPLETE;
            if (MatchesAny(msg, TaskDeleteTriggers)) return Intent.TASK_DELETE;
            if (MatchesAny(msg, TaskViewTriggers)) return Intent.TASK_VIEW;
            if (MatchesAny(msg, ReminderTriggers)) return Intent.REMINDER_SET;
            if (MatchesAny(msg, TaskAddTriggers)) return Intent.TASK_ADD;

            return Intent.UNKNOWN;
        }

        // ── EXTRACT TASK TITLE ───────────────────────────────────
        // Pulls the task subject out of a natural-language sentence.
        // e.g. "add a task to enable 2FA" → "Enable 2FA"
        //      "remind me to update my password" → "Update my password"
        public string ExtractTaskTitle(string input)
        {
            string msg = input.Trim();

            // Strip common leading phrases using regex
            string[] stripPatterns =
            {
                @"(?i)^(add a task to |add task to |create a task to |new task to |remind me to |can you remind me to |set a task to |i need to |i want to |make a note to |note that |set up task |schedule task |task to )",
                @"(?i)^(add a task |add task |create task |new task |create a task )"
            };

            foreach (string pattern in stripPatterns)
            {
                string cleaned = Regex.Replace(msg, pattern, "").Trim();
                if (!string.IsNullOrWhiteSpace(cleaned) && cleaned.Length < msg.Length)
                {
                    // Capitalise first letter
                    return char.ToUpper(cleaned[0]) + cleaned.Substring(1);
                }
            }

            // Fallback — return the whole input capitalised
            return char.ToUpper(msg[0]) + msg.Substring(1);
        }

        // ── EXTRACT TASK ID FROM MESSAGE ─────────────────────────
        // e.g. "complete task 3" → 3
        // Returns -1 if no number found.
        public int ExtractTaskId(string input)
        {
            Match match = Regex.Match(input, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int id))
                return id;
            return -1;
        }

        // ── EXTRACT REMINDER TIMEFRAME ───────────────────────────
        // e.g. "remind me in 3 days" → "in 3 days"
        //      "remind me tomorrow"   → "tomorrow"
        public string ExtractReminderTime(string input)
        {
            string msg = input.ToLower();

            // Look for "in X days/weeks/hours"
            Match match = Regex.Match(msg, @"in\s+(\d+)\s+(day|days|week|weeks|hour|hours|month|months)");
            if (match.Success)
                return $"in {match.Groups[1].Value} {match.Groups[2].Value}";

            // Look for named times
            if (msg.Contains("tomorrow")) return "tomorrow";
            if (msg.Contains("next week")) return "next week";
            if (msg.Contains("this week")) return "this week";
            if (msg.Contains("tonight")) return "tonight";

            // Look for specific dates
            Match dateMatch = Regex.Match(msg, @"\d{4}-\d{2}-\d{2}|\d{1,2}/\d{1,2}");
            if (dateMatch.Success) return dateMatch.Value;

            return "soon"; // fallback
        }

        // ── HELPER — MATCHES ANY ─────────────────────────────────
        // Returns true if the message contains ANY of the trigger phrases.
        private bool MatchesAny(string msg, string[] triggers)
        {
            foreach (string trigger in triggers)
                if (msg.Contains(trigger))
                    return true;
            return false;
        }
    }
}