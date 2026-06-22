using System;
using System.Collections.Generic;
using System.Text;

namespace CyberSecurityChatbot_GUI3
{
    // ============================================================
    // CHATBOT CLASS Part 3
    //
    // NEW IN PART 3:
    //   Task 1  — Task assistant commands (add/view/complete/delete)
    //   Task 2  — Quiz integration (start quiz, answer routing)
    //   Task 3  — NLP Processor (intent detection before keyword scan)
    //   Task 4  — Activity log (log every significant action)
    //
    // STILL PRESENT FROM PART 2:
    //   Req 2  — Keyword Recognition (6+ topics)
    //   Req 3  — Random Responses
    //   Req 4  — Conversation Flow ("tell me more")
    //   Req 5  — Memory & Recall (favourite topic)
    //   Req 6  — Sentiment Detection
    //   Req 7  — Error Handling
    //   Req 8  — Code Optimisation (OOP, Dictionary, private methods)
    // ============================================================
    public class Chatbot
    {
        // ── PART 3 DEPENDENCIES ──────────────────────────────────
        private readonly DatabaseManager _db;
        private readonly QuizManager _quiz;
        private readonly ActivityLogger _logger;
        private readonly NLPProcessor _nlp;

        // State for the "would you like a reminder?" follow-up
        private bool _awaitingReminder = false;
        private int _lastAddedTaskId = -1;

        // ============================================================
        // REQUIREMENT 3 — RANDOM RESPONSES (unchanged from Part 2)
        // ============================================================
        private readonly Dictionary<string, string[]> _randomTips = new Dictionary<string, string[]>
        {
            {
                "phishing", new[]
                {
                    "Be cautious of emails asking for personal information — scammers disguise themselves as trusted organisations.",
                    "Always check the sender's email address carefully. 'paypa1.com' is fake — 'paypal.com' is real.",
                    "Real banks and companies will NEVER ask for your password or PIN via email.",
                    "Watch for urgent language like 'ACT NOW!' or 'Your account will be closed' — classic red flags!",
                    "Hover over links before clicking — your browser shows the real destination at the bottom of the screen."
                }
            },
            {
                "password", new[]
                {
                    "Use strong, unique passwords for every account — never reuse the same password across sites.",
                    "A passphrase like 'Blue-Horse-Sunshine-Rain!' is easier to remember and harder to crack than 'P@ssw0rd'.",
                    "Never reuse passwords — if one site is hacked, all your other accounts become vulnerable.",
                    "Use a free password manager like Bitwarden — it remembers all passwords, you only need one master password.",
                    "Enable two-factor authentication (2FA) — even if your password leaks, hackers still can't get in!"
                }
            },
            {
                "privacy", new[]
                {
                    "Review your social media privacy settings — limit who can see your posts and personal information.",
                    "Avoid sharing your full birthdate, ID number, or home address on public platforms.",
                    "Use private/incognito browsing mode when using shared or public computers.",
                    "Only fill in personal information on online forms that is truly necessary — less is safer.",
                    "Regularly check which apps have access to your location, camera, and contacts — revoke what you don't need."
                }
            },
            {
                "scam", new[]
                {
                    "If an offer sounds too good to be true, it almost certainly is a scam.",
                    "Prize scams are very common — no legitimate company asks you to pay fees to claim a prize.",
                    "Verify requests for money transfers by calling the person directly on a known, verified number.",
                    "In South Africa, report scams to SABRIC: 0861 022 339 or report@cybersecurity.gov.za.",
                    "Never send money or gift cards to someone you've only met online — this is a classic scam tactic."
                }
            },
            {
                "virus", new[]
                {
                    "Keep your antivirus software updated — new viruses are created every single day.",
                    "Only download software from official, trusted websites — never from random pop-ups.",
                    "If your computer suddenly slows down or shows strange pop-ups, run a full antivirus scan immediately.",
                    "Never plug in an unknown USB drive — it could silently install malware on your computer.",
                    "Back up your important files regularly — if ransomware strikes, you won't lose everything."
                }
            },
            {
                "browsing", new[]
                {
                    "Always check for the padlock icon 🔒 and 'https://' before entering any personal information.",
                    "Avoid clicking pop-up ads — 'You won an iPhone!' is always a scam.",
                    "Keep your browser updated — updates patch security holes that hackers exploit.",
                    "Don't save passwords in your browser — use a dedicated password manager instead.",
                    "Use a VPN on public WiFi — it encrypts your connection so hackers can't steal your data."
                }
            }
        };

        // ── CONSTRUCTOR ──────────────────────────────────────────
        public Chatbot()
        {
            _db = new DatabaseManager();
            _quiz = new QuizManager();
            _logger = new ActivityLogger();
            _nlp = new NLPProcessor();
        }

        // Expose logger so MainWindow can read it if needed
        public ActivityLogger Logger => _logger;

        // ============================================================
        // RANDOM TIP SELECTOR (unchanged from Part 2)
        // ============================================================
        private string GetRandomTip(string topic)
        {
            if (_randomTips.ContainsKey(topic))
            {
                var rand = new Random();
                string[] tips = _randomTips[topic];
                return tips[rand.Next(tips.Length)];
            }
            return "Always think before you click — when in doubt, don't! 🛡️";
        }

        // ============================================================
        // MAIN GET RESPONSE — called from MainWindow on every message
        //
        // NEW ORDER OF CHECKS FOR PART 3:
        //   0. Empty input guard
        //   0b. Quiz answer routing (if quiz is active)
        //   0c. Reminder follow-up (if waiting for yes/no)
        //   1. NLP intent detection  ← NEW (catches tasks, quiz, log)
        //   2. Sentiment detection   (from Part 2)
        //   3. Conversation flow     (from Part 2)
        //   4. Explicit interest     (from Part 2)
        //   5. Menu / help
        //   6. Small talk
        //   7. Topic keyword recognition (from Part 2)
        //   8. Memory-based fallback
        //   9. Generic fallback
        // ============================================================
        public string GetResponse(string input, User user)
        {
            string msg = input.ToLower().Trim();
            string name = user.Name;

            // ── 0. EMPTY INPUT GUARD ─────────────────────────────
            if (string.IsNullOrWhiteSpace(msg))
                return $"Please type something so I can help you, {name}! 😊";

            // ── 0b. QUIZ ANSWER ROUTING ──────────────────────────
            // If the quiz is active, ALL input goes to the quiz first.
            if (_quiz.IsActive && _quiz.AwaitingAnswer)
            {
                string quizResult = _quiz.SubmitAnswer(input.Trim());
                if (quizResult != null)
                {
                    if (!_quiz.IsActive)
                        _logger.Log($"Quiz completed — Score: {_quiz.Score}/{_quiz.TotalQuestions}");
                    return quizResult;
                }
            }

            // ── 0c. REMINDER FOLLOW-UP ───────────────────────────
            // After adding a task, the bot asks "Would you like a reminder?"
            // The next message is captured here.
            if (_awaitingReminder)
            {
                _awaitingReminder = false;

                if (msg.Contains("yes") || msg.Contains("yeah") || msg.Contains("sure") ||
                    msg.Contains("ok") || msg.Contains("please") || msg.StartsWith("remind"))
                {
                    // Check if they specified a timeframe
                    string when = _nlp.ExtractReminderTime(input);

                    if (_lastAddedTaskId >= 0)
                    {
                        _db.UpdateReminder(_lastAddedTaskId, $"Reminder set for {when}");
                        _logger.Log($"Reminder set for task ID {_lastAddedTaskId}: {when}");
                        return $"Got it, {name}! I'll remind you {when}. ⏰\n\n" +
                               "Type 'show tasks' to see all your tasks.";
                    }
                }
                else if (msg.Contains("no") || msg.Contains("nope") || msg.Contains("skip"))
                {
                    return $"No problem, {name}! The task has been saved without a reminder. 😊\n" +
                           "Type 'show tasks' to see all your tasks.";
                }
                else
                {
                    // Try to extract a time from this message anyway
                    string when = _nlp.ExtractReminderTime(input);
                    if (_lastAddedTaskId >= 0)
                    {
                        _db.UpdateReminder(_lastAddedTaskId, $"Reminder set for {when}");
                        _logger.Log($"Reminder set for task ID {_lastAddedTaskId}: {when}");
                        return $"Got it! Reminder set for {when}. ⏰\n\nType 'show tasks' anytime!";
                    }
                }
            }

            // ── 1. NLP INTENT DETECTION ──────────────────────────
            Intent intent = _nlp.DetectIntent(msg);

            switch (intent)
            {
                case Intent.QUIZ_START:
                    _logger.Log("Quiz started");
                    return $"🧠 Let's test your cybersecurity knowledge, {name}!\n\n" +
                           _quiz.StartQuiz();

                case Intent.ACTIVITY_LOG:
                    _logger.Log("Activity log viewed");
                    return _logger.GetFormattedLog(showAll: false);

                case Intent.ACTIVITY_LOG_FULL:
                    return _logger.GetFormattedLog(showAll: true);

                case Intent.TASK_ADD:
                    return HandleTaskAdd(input, user);

                case Intent.TASK_VIEW:
                    return HandleTaskView(name);

                case Intent.TASK_COMPLETE:
                    return HandleTaskComplete(input, name);

                case Intent.TASK_DELETE:
                    return HandleTaskDelete(input, name);

                case Intent.REMINDER_SET:
                    // If they said "remind me in 3 days" without adding a task first
                    if (_lastAddedTaskId >= 0)
                    {
                        string when = _nlp.ExtractReminderTime(input);
                        _db.UpdateReminder(_lastAddedTaskId, $"Reminder: {when}");
                        _logger.Log($"Reminder updated for task ID {_lastAddedTaskId}: {when}");
                        return $"⏰ Reminder set for {when} on your last task!";
                    }
                    return $"Please add a task first, {name}! Type 'add task [description]'.";
            }

            // ── 2. SENTIMENT DETECTION (Part 2 preserved) ────────
            string sentiment = DetectSentiment(msg, name);
            if (sentiment != null)
            {
                string tipAfterSentiment = GetTopicTipFromContext(msg, user);
                return sentiment + tipAfterSentiment;
            }

            // ── 3. CONVERSATION FLOW (Part 2 preserved) ──────────
            if (msg.Contains("tell me more") || msg.Contains("explain more") ||
                msg.Contains("more info") || msg.Contains("another tip") ||
                msg.Contains("give me another") || msg.Contains("more details") ||
                msg.Contains("keep going") || msg.Contains("continue"))
            {
                if (!string.IsNullOrEmpty(user.LastTopic))
                {
                    user.RecordTopicMention(user.LastTopic);
                    return $"Sure {name}! Here's another tip on {user.LastTopic}:\n\n" +
                           GetRandomTip(user.LastTopic) +
                           $"\n\nWant even more? Just say 'tell me more' again! 😊";
                }
                return $"We haven't discussed a topic yet, {name}. " +
                       "Try asking about phishing, passwords, privacy, scams, or viruses!";
            }

            // ── 4. EXPLICIT INTEREST (Part 2 preserved) ──────────
            if (msg.Contains("i'm interested in") || msg.Contains("im interested in") ||
                msg.Contains("i am interested in") || msg.Contains("my favourite topic") ||
                msg.Contains("my favorite topic") || msg.Contains("i love learning about") ||
                msg.Contains("i want to learn about") || msg.Contains("tell me about"))
            {
                return HandleExplicitInterest(msg, user);
            }

            // ── 5. MENU / HELP ────────────────────────────────────
            if (msg == "menu" || msg == "help" || msg.Contains("what can you do") ||
                msg.Contains("what topics") || msg.Contains("show topics"))
            {
                return GetMenu();
            }

            // ── 6. SMALL TALK (Part 2 preserved) ─────────────────
            if (msg.Contains("how are you") || msg.Contains("how you feeling") ||
                msg.Contains("what is your purpose") || msg.Contains("who are you"))
            {
                string[] feelings = {
                    $"I'm doing great {name}! 😊 I can chat about cybersecurity, manage your tasks, and even quiz you!",
                    $"Fantastic {name}! My purpose is to help you stay safe online AND manage your cyber tasks. Try 'add task'!",
                    $"Super excited {name}! 🌟 Type 'menu' to see everything I can do!"
                };
                return feelings[new Random().Next(feelings.Length)];
            }

            if (msg.Contains("thank") || msg.Contains("thanks") || msg.Contains("appreciate"))
                return $"You're very welcome, {name}! 😊 Stay safe online! Anything else you'd like to learn or do?";

            // ── 7. TOPIC KEYWORD RECOGNITION (Part 2 preserved) ──
            if (msg.Contains("phish") || msg.Contains("scam email") || msg.Contains("fake email"))
            {
                user.LastTopic = "phishing"; user.RecordTopicMention("phishing");
                _logger.Log($"Topic discussed: phishing");
                return GetPhishingResponse(msg, name);
            }
            if (msg.Contains("password") || msg.Contains("passphrase") ||
                msg.Contains("2fa") || msg.Contains("two factor"))
            {
                user.LastTopic = "password"; user.RecordTopicMention("password");
                _logger.Log($"Topic discussed: password security");
                return GetPasswordResponse(msg, name);
            }
            if (msg.Contains("privacy") || msg.Contains("private") || msg.Contains("personal info"))
            {
                user.LastTopic = "privacy"; user.RecordTopicMention("privacy");
                _logger.Log($"Topic discussed: privacy");
                return GetPrivacyResponse(msg, name);
            }
            if (msg.Contains("scam") || msg.Contains("fraud") || msg.Contains("trick"))
            {
                user.LastTopic = "scam"; user.RecordTopicMention("scam");
                _logger.Log($"Topic discussed: scams");
                return GetScamResponse(msg, name);
            }
            if (msg.Contains("virus") || msg.Contains("malware") ||
                msg.Contains("ransomware") || msg.Contains("antivirus"))
            {
                user.LastTopic = "virus"; user.RecordTopicMention("virus");
                _logger.Log($"Topic discussed: viruses/malware");
                return GetVirusResponse(msg, name);
            }
            if (msg.Contains("link") || msg.Contains("url") || msg.Contains("click"))
            {
                user.LastTopic = "phishing"; user.RecordTopicMention("phishing");
                return GetLinkResponse(msg, name);
            }
            if (msg.Contains("browse") || msg.Contains("internet") ||
                msg.Contains("website") || msg.Contains("web") || msg.Contains("https"))
            {
                user.LastTopic = "browsing"; user.RecordTopicMention("browsing");
                _logger.Log($"Topic discussed: safe browsing");
                return GetBrowsingResponse(msg, name);
            }
            if (msg.Contains("report") || msg.Contains("saps") || msg.Contains("police"))
            {
                user.LastTopic = "scam"; user.RecordTopicMention("scam");
                return GetReportingResponse(msg, name);
            }
            if (msg.Contains("cyber") || msg.Contains("security") ||
                msg.Contains("protect") || msg.Contains("safe"))
            {
                user.LastTopic = "phishing"; user.RecordTopicMention("phishing");
                return GetCyberResponse(msg, name);
            }

            // ── 8. MEMORY-BASED FALLBACK (Part 2 preserved) ──────
            if (!string.IsNullOrEmpty(user.FavouriteTopic))
            {
                return $"Hmm {name}, I didn't quite catch that. 🤔\n\n" +
                       $"Since you're interested in {user.FavouriteTopic}, here's a tip:\n\n" +
                       GetRandomTip(user.FavouriteTopic) +
                       "\n\nOr type 'menu' to explore all topics, or 'add task' to manage tasks!";
            }

            // ── 9. GENERIC FALLBACK ───────────────────────────────
            return $"Hmm {name}, I'm not sure I understood that. 🤔\n" +
                   "Try: 'add task', 'show tasks', 'start quiz', 'show activity log'\n" +
                   "Or ask about phishing, passwords, privacy, scams, or viruses.\n" +
                   "Type 'menu' to see everything I can help with!";
        }

        // ============================================================
        // TASK HANDLERS — Task 1 (Part 3)
        // ============================================================

        // ── ADD TASK ─────────────────────────────────────────────
        private string HandleTaskAdd(string input, User user)
        {
            // Extract the task title using NLP
            string rawTitle = _nlp.ExtractTaskTitle(input);

            // Generate a smart description based on the title
            string description = GenerateTaskDescription(rawTitle);

            var task = new TaskItem(rawTitle, description);
            int newId = _db.AddTask(task);
            _lastAddedTaskId = newId;
            _awaitingReminder = true;

            _logger.Log($"Task added: '{rawTitle}' (ID: {newId})");

            return $"✅ Task added successfully, {user.Name}!\n\n" +
                   $"📝 Task: {rawTitle}\n" +
                   $"📋 Description: {description}\n" +
                   $"🆔 Task ID: {newId}\n\n" +
                   "Would you like to set a reminder? (e.g. 'Yes, remind me in 3 days' or 'No')";
        }

        // Generates a helpful cybersecurity description based on keywords in the title
        private string GenerateTaskDescription(string title)
        {
            string t = title.ToLower();
            if (t.Contains("2fa") || t.Contains("two factor") || t.Contains("two-factor"))
                return "Enable Two-Factor Authentication to add an extra layer of security to your accounts.";
            if (t.Contains("password"))
                return "Update or strengthen your password using a passphrase with mixed characters.";
            if (t.Contains("privacy") || t.Contains("settings"))
                return "Review account privacy settings to ensure your personal data is protected.";
            if (t.Contains("antivirus") || t.Contains("virus") || t.Contains("malware"))
                return "Run a full antivirus scan and ensure your software is up to date.";
            if (t.Contains("backup") || t.Contains("back up"))
                return "Back up your important files to a secure location (external drive or cloud).";
            if (t.Contains("update") || t.Contains("patch"))
                return "Apply the latest software updates and security patches to your device.";
            if (t.Contains("vpn"))
                return "Set up a VPN to encrypt your internet connection, especially on public WiFi.";
            // Default description
            return $"Complete cybersecurity task: {title}. Stay safe online!";
        }

        // ── VIEW TASKS ───────────────────────────────────────────
        private string HandleTaskView(string name)
        {
            var tasks = _db.GetAllTasks();
            _logger.Log("Task list viewed");

            if (tasks.Count == 0)
                return $"📋 You have no tasks yet, {name}!\n\n" +
                       "Try: 'Add task - Enable two-factor authentication'\n" +
                       "Or: 'Add a task to review my privacy settings'";

            var sb = new StringBuilder();
            sb.AppendLine($"📋 ═══ YOUR CYBERSECURITY TASKS ({tasks.Count}) ═══\n");

            int pending = 0;
            int completed = 0;

            foreach (var task in tasks)
            {
                sb.AppendLine(task.ToString());
                sb.AppendLine();
                if (task.IsCompleted) completed++;
                else pending++;
            }

            sb.AppendLine("─────────────────────────────");
            sb.AppendLine($"✅ Completed: {completed}   🔲 Pending: {pending}");
            sb.AppendLine();
            sb.AppendLine("Commands:\n  'complete task [ID]'  — mark done\n  'delete task [ID]'    — remove task");

            return sb.ToString().TrimEnd();
        }

        // ── COMPLETE TASK ─────────────────────────────────────────
        private string HandleTaskComplete(string input, string name)
        {
            int id = _nlp.ExtractTaskId(input);
            if (id < 0)
                return $"Please include the Task ID, {name}. Example: 'complete task 2'\n" +
                       "Type 'show tasks' to see all IDs.";

            bool success = _db.MarkTaskComplete(id);
            if (success)
            {
                _logger.Log($"Task ID {id} marked as complete");
                return $"✅ Task ID {id} marked as complete! Well done, {name}! 🎉\n" +
                       "Type 'show tasks' to see your updated list.";
            }
            return $"❌ Couldn't find task ID {id}. Type 'show tasks' to see valid IDs.";
        }

        // ── DELETE TASK ───────────────────────────────────────────
        private string HandleTaskDelete(string input, string name)
        {
            int id = _nlp.ExtractTaskId(input);
            if (id < 0)
                return $"Please include the Task ID, {name}. Example: 'delete task 2'\n" +
                       "Type 'show tasks' to see all IDs.";

            bool success = _db.DeleteTask(id);
            if (success)
            {
                _logger.Log($"Task ID {id} deleted");
                return $"🗑️ Task ID {id} deleted, {name}.\n" +
                       "Type 'show tasks' to see your remaining tasks.";
            }
            return $"❌ Couldn't find task ID {id}. Type 'show tasks' to see valid IDs.";
        }

        // ============================================================
        // SENTIMENT DETECTION (unchanged from Part 2)
        // ============================================================
        private string DetectSentiment(string msg, string name)
        {
            if (msg.Contains("worried") || msg.Contains("scared") ||
                msg.Contains("nervous") || msg.Contains("afraid") || msg.Contains("fear"))
                return $"It's completely understandable to feel that way, {name}. 💙 " +
                       "Scammers can be very convincing. Let me share a tip to help you stay safe:\n\n";

            if (msg.Contains("curious") || msg.Contains("wondering") || msg.Contains("want to know"))
                return $"I love your curiosity, {name}! 🌟 " +
                       "That's exactly the right mindset for staying safe online. Here's something useful:\n\n";

            if (msg.Contains("frustrated") || msg.Contains("angry") || msg.Contains("annoyed") ||
                msg.Contains("confused") || msg.Contains("don't understand") || msg.Contains("lost"))
                return $"I hear you, {name} — cybersecurity can feel overwhelming at first. 😌 " +
                       "Take a breath! Let me break it down simply:\n\n";

            if (msg.Contains("happy") || msg.Contains("excited") || msg.Contains("love this"))
                return $"Love the energy, {name}! 🎉 Let's keep that momentum going:\n\n";

            if (msg.Contains("tired") || msg.Contains("overwhelmed") || msg.Contains("exhausted"))
                return $"Take it easy, {name}. 😊 Here's just one quick tip:\n\n";

            return null;
        }

        private string GetTopicTipFromContext(string msg, User user)
        {
            if (msg.Contains("phish") || msg.Contains("email"))
            { user.LastTopic = "phishing"; user.RecordTopicMention("phishing"); return GetRandomTip("phishing"); }
            if (msg.Contains("password") || msg.Contains("login"))
            { user.LastTopic = "password"; user.RecordTopicMention("password"); return GetRandomTip("password"); }
            if (msg.Contains("privacy") || msg.Contains("private"))
            { user.LastTopic = "privacy"; user.RecordTopicMention("privacy"); return GetRandomTip("privacy"); }
            if (msg.Contains("scam") || msg.Contains("fraud"))
            { user.LastTopic = "scam"; user.RecordTopicMention("scam"); return GetRandomTip("scam"); }
            if (msg.Contains("virus") || msg.Contains("malware"))
            { user.LastTopic = "virus"; user.RecordTopicMention("virus"); return GetRandomTip("virus"); }

            return "Always think before you click — when in doubt, don't! 🛡️\n\n" +
                   "Type 'menu' to explore all cybersecurity topics.";
        }

        // ============================================================
        // EXPLICIT INTEREST HANDLER (unchanged from Part 2)
        // ============================================================
        private string HandleExplicitInterest(string msg, User user)
        {
            string topic = DetectTopicFromMessage(msg);
            if (!string.IsNullOrEmpty(topic))
            {
                bool alreadyFavourite = user.FavouriteTopic == topic;
                user.SetFavouriteTopic(topic);
                user.LastTopic = topic;
                _logger.Log($"Favourite topic set: {topic}");

                if (alreadyFavourite)
                    return $"I already remember that you're interested in {topic}, {user.Name}! 🧠\n\n" +
                           $"Here's another tip:\n\n{GetRandomTip(topic)}\n\nType 'tell me more'!";

                return $"Got it, {user.Name}! I'll remember that you're interested in {topic}. 🧠\n\n" +
                       GetRandomTip(topic) +
                       $"\n\nI'll keep {topic} in mind. Type 'tell me more' anytime!";
            }
            return $"That sounds interesting, {user.Name}! I specialise in cybersecurity. Try:\n\n" +
                   "• 'I'm interested in phishing'\n• 'I'm interested in passwords'\n" +
                   "• 'I'm interested in privacy'\n• 'I'm interested in scams'";
        }

        private string DetectTopicFromMessage(string msg)
        {
            if (msg.Contains("phish") || msg.Contains("email")) return "phishing";
            if (msg.Contains("password") || msg.Contains("passphrase")) return "password";
            if (msg.Contains("privacy") || msg.Contains("private")) return "privacy";
            if (msg.Contains("scam") || msg.Contains("fraud")) return "scam";
            if (msg.Contains("virus") || msg.Contains("malware")) return "virus";
            if (msg.Contains("browse") || msg.Contains("web")) return "browsing";
            return "";
        }

        // ============================================================
        // MENU — updated with Part 3 features
        // ============================================================
        private string GetMenu()
        {
            return "═══ SIYA'S CYBER CHATBOT — FULL MENU ═══\n\n" +
                   "📚 CYBERSECURITY TOPICS:\n" +
                   "  🎣 Phishing      — Spot fake emails\n" +
                   "  🔑 Passwords     — Create strong passwords\n" +
                   "  🔒 Privacy       — Protect your information\n" +
                   "  ⚠️  Scams         — Recognise and avoid scams\n" +
                   "  🦠 Viruses       — Protect against malware\n" +
                   "  🔗 Links         — Spot fake links\n" +
                   "  🌐 Browsing      — Stay safe online\n" +
                   "  📋 Reporting     — Report cybercrime in SA\n\n" +
                   "📝 TASK ASSISTANT:\n" +
                   "  'add task [description]'    — Add a new task\n" +
                   "  'show tasks'                — View all tasks\n" +
                   "  'complete task [ID]'        — Mark a task done\n" +
                   "  'delete task [ID]'          — Remove a task\n\n" +
                   "🧠 QUIZ:\n" +
                   "  'start quiz'                — Test your knowledge!\n\n" +
                   "📋 ACTIVITY LOG:\n" +
                   "  'show activity log'         — See recent actions\n" +
                   "  'show full log'             — See all actions\n\n" +
                   "Just type naturally — I'll understand! 😊";
        }

        // ============================================================
        // TOPIC RESPONSE METHODS (unchanged from Part 2)
        // ============================================================
        private string GetPhishingResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define"))
                return $"Great question {name}! 🎣\n\nPHISHING is when scammers send fake emails pretending to be from real companies " +
                       "like banks, Netflix, or PayPal.\n\nWant to learn how to SPOT them? Just ask!";
            if (msg.Contains("spot") || msg.Contains("identify") || msg.Contains("recognise"))
                return $"HOW TO SPOT PHISHING EMAILS, {name}:\n\n" +
                       "1️⃣ Check the sender's email — 'paypa1.com' is FAKE\n" +
                       "2️⃣ Look for spelling mistakes\n3️⃣ Watch for URGENT language\n" +
                       "4️⃣ Hover over links before clicking\n5️⃣ If too good to be true — it's a scam!";
            if (msg.Contains("tip") || msg.Contains("give me"))
                return $"Phishing tip for you, {name}:\n\n{GetRandomTip("phishing")}\n\nType 'tell me more'!";
            return $"Here's something about phishing, {name}: 🎣\n\n{GetRandomTip("phishing")}\n\n" +
                   "Ask: 'What is phishing?' or 'How to spot phishing?'";
        }

        private string GetPasswordResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define"))
                return $"A PASSWORD is your digital key, {name}! 🔑\n\nA weak password is like leaving your front door unlocked.\nWant to learn how to create a strong one?";
            if (msg.Contains("create") || msg.Contains("strong") || msg.Contains("how to"))
                return $"PASSWORD TIPS, {name}! 🦸\n\n✅ 12+ characters\n✅ Mix CAPITALS, lowercase, numbers, symbols\n✅ Use a passphrase: 'Blue-Horse-Sunshine-Rain!'\n✅ Different password for every account\n\n❌ BAD: 'password123'  ✅ GOOD: 'MyD0g!sC00l2024@'";
            if (msg.Contains("2fa") || msg.Contains("two factor"))
                return $"2FA adds an extra layer, {name}! 🔐\n\nEven if a hacker steals your password, they can't log in without the second factor — usually a code sent to your phone.\n\nEnable 2FA on your email, banking, and social media NOW!";
            return $"Password tip for you, {name}: 🔑\n\n{GetRandomTip("password")}\n\nType 'tell me more' for another!";
        }

        private string GetPrivacyResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define"))
                return $"PRIVACY online means controlling who can see your information, {name}! 🔒\n\nProtect your name, ID number, location, and photos.\nWant a quick privacy tip?";
            return $"Privacy tip for you, {name}: 🔒\n\n{GetRandomTip("privacy")}\n\nType 'tell me more'!";
        }

        private string GetScamResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define"))
                return $"A SCAM is a dishonest scheme to trick you, {name}! ⚠️\n\nCommon types: prize scams, romance scams, job scams.\nWant to know how to spot and avoid them?";
            return $"Scam tip for you, {name}: ⚠️\n\n{GetRandomTip("scam")}\n\nType 'tell me more'!";
        }

        private string GetVirusResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define"))
                return $"A VIRUS is malicious software, {name}! 🦠\n\nTypes include ransomware, spyware, and trojans.\nWant tips on how to protect yourself?";
            return $"Virus protection tip, {name}: 🦠\n\n{GetRandomTip("virus")}\n\nType 'tell me more'!";
        }

        private string GetLinkResponse(string msg, string name)
        {
            if (msg.Contains("spot") || msg.Contains("fake") || msg.Contains("how to"))
                return $"HOW TO SPOT FAKE LINKS, {name}: 🕵️\n\n1️⃣ HOVER over the link (DON'T click!) — see real destination\n2️⃣ Spelling tricks — 'faceb00k.com' is FAKE\n3️⃣ Check for 'https://'\n4️⃣ Shortened links like bit.ly hide the real destination";
            return $"Stay alert about links, {name}! 🔗\n\nAsk: 'How to spot fake links?'";
        }

        private string GetBrowsingResponse(string msg, string name)
        {
            if (msg.Contains("tip") || msg.Contains("how to"))
                return $"Safe browsing tip, {name}: 🌐\n\n{GetRandomTip("browsing")}\n\nType 'tell me more'!";
            return $"Safe browsing tip, {name}: 🌐\n\n{GetRandomTip("browsing")}\n\nAsk: 'How to browse safely?'";
        }

        private string GetReportingResponse(string msg, string name)
        {
            if (msg.Contains("saps") || msg.Contains("police"))
                return $"Reporting to SAPS, {name}: 🚔\n\n📞 Crime Stop: 0860 010 111 (24/7, anonymous)\n🏢 Visit your local police station — ask for the Cybercrime Unit\n📁 Bring ALL evidence: emails, screenshots, bank statements";
            return $"WHERE TO REPORT IN SOUTH AFRICA, {name}: 📋\n\n📧 report@cybersecurity.gov.za\n🏦 Your Bank fraud department\n📞 SABRIC: 0861 022 339\n\nKeep all evidence — don't delete emails!";
        }

        private string GetCyberResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define"))
                return $"CYBERSECURITY is protecting yourself online, {name}! 🛡️\n\nIt's like having:\n🔒 A lock on your digital door (passwords)\n🛡️ A security guard (antivirus)\n👁️ Training to spot danger (awareness)\n\nWant to learn WHY it's important?";
            if (msg.Contains("why") || msg.Contains("important"))
                return $"Cyber attacks in South Africa increased 200% recently, {name}! 🌍\n\nCybersecurity protects:\n💰 YOUR MONEY — bank accounts\n🪪 YOUR IDENTITY — personal information\n💻 YOUR DEVICES — computer and phone\n🔒 YOUR PRIVACY — photos and messages";
            return $"Type 'menu' to see all cybersecurity topics, {name}! 🛡️";
        }
    }
}