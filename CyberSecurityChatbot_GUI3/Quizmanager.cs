using System;
using System.Collections.Generic;

namespace CyberSecurityChatbot_GUI3
{
    // ============================================================
    // QUIZ QUESTION — holds one question with its options & answer
    // ============================================================
    public class QuizQuestion
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }  // null = True/False question
        public string CorrectAnswer { get; set; }  // "A","B","C","D" or "True"/"False"
        public string Explanation { get; set; }
        public bool IsMultipleChoice => Options != null && Options.Count > 0;
    }

    // ============================================================
    //
    // FEATURES:
    //   - 12 questions (rubric requires 10+)
    //   - Mix of multiple-choice AND true/false (rubric requirement)
    //   - Immediate feedback after each answer
    //   - Score tracking — displayed at the end
    //   - Covers: phishing, passwords, safe browsing, social engineering
    // ============================================================
    public class QuizManager
    {
        private readonly List<QuizQuestion> _questions;
        private int _currentIndex = 0;
        private int _score = 0;
        private bool _isActive = false;
        private bool _awaitingAnswer = false;

        // Track whether we're waiting for a reminder response after a task is added
        public bool IsActive => _isActive;
        public bool AwaitingAnswer => _awaitingAnswer;
        public int CurrentIndex => _currentIndex;
        public int Score => _score;
        public int TotalQuestions => _questions.Count;

        // ── CONSTRUCTOR — define all questions here ──────────────
        public QuizManager()
        {
            _questions = new List<QuizQuestion>
            {
                // ── QUESTION 1 — Multiple Choice ─────────────────
                new QuizQuestion
                {
                    Question = "What is PHISHING?",
                    Options  = new List<string>
                    {
                        "A) A type of fishing sport",
                        "B) A scam where criminals send fake emails to steal your information",
                        "C) A way to speed up your computer",
                        "D) A new social media platform"
                    },
                    CorrectAnswer = "B",
                    Explanation   = "Phishing uses fake emails that look legitimate to trick you into giving away passwords or personal information."
                },

                // ── QUESTION 2 — True/False ──────────────────────
                new QuizQuestion
                {
                    Question      = "TRUE or FALSE: You should use the same password for all your accounts because it's easier to remember.",
                    Options       = null, // True/False question
                    CorrectAnswer = "False",
                    Explanation   = "NEVER reuse passwords! If one account is hacked, all your other accounts become vulnerable too."
                },

                // ── QUESTION 3 — Multiple Choice ─────────────────
                new QuizQuestion
                {
                    Question = "Which of the following is the STRONGEST password?",
                    Options  = new List<string>
                    {
                        "A) password123",
                        "B) MyName1990",
                        "C) Blue-Horse-Sunshine-Rain!7",
                        "D) qwerty"
                    },
                    CorrectAnswer = "C",
                    Explanation   = "A long passphrase with mixed characters is much harder to crack than short passwords or common words."
                },

                // ── QUESTION 4 — True/False ──────────────────────
                new QuizQuestion
                {
                    Question      = "TRUE or FALSE: The padlock icon (🔒) in your browser means the website is completely safe and trustworthy.",
                    Options       = null,
                    CorrectAnswer = "False",
                    Explanation   = "The padlock means the connection is ENCRYPTED — but scam websites can also have padlocks! Always check the URL carefully."
                },

                // ── QUESTION 5 — Multiple Choice ─────────────────
                new QuizQuestion
                {
                    Question = "You receive an email saying 'Your bank account is SUSPENDED! Click here NOW to verify.' What should you do?",
                    Options  = new List<string>
                    {
                        "A) Click the link immediately — it sounds urgent!",
                        "B) Reply with your account number",
                        "C) Delete the email and call your bank directly on their official number",
                        "D) Forward it to all your friends"
                    },
                    CorrectAnswer = "C",
                    Explanation   = "Urgency is a classic phishing trick! Never click links in suspicious emails. Always contact your bank using their official number."
                },

                // ── QUESTION 6 — True/False ──────────────────────
                new QuizQuestion
                {
                    Question      = "TRUE or FALSE: Two-Factor Authentication (2FA) means a hacker who has your password can still NOT log into your account.",
                    Options       = null,
                    CorrectAnswer = "True",
                    Explanation   = "2FA adds a second step (like a code sent to your phone). Even with your password, the hacker can't get in without that second code!"
                },

                // ── QUESTION 7 — Multiple Choice ─────────────────
                new QuizQuestion
                {
                    Question = "What does RANSOMWARE do?",
                    Options  = new List<string>
                    {
                        "A) Speeds up your internet connection",
                        "B) Locks your files and demands payment to unlock them",
                        "C) Improves your computer's graphics",
                        "D) Scans for viruses automatically"
                    },
                    CorrectAnswer = "B",
                    Explanation   = "Ransomware encrypts your files and criminals demand payment (a ransom) to give you access back. Always back up your files!"
                },

                // ── QUESTION 8 — True/False ──────────────────────
                new QuizQuestion
                {
                    Question      = "TRUE or FALSE: It is safe to use public WiFi at a coffee shop to do your online banking.",
                    Options       = null,
                    CorrectAnswer = "False",
                    Explanation   = "Public WiFi is UNSECURED. Hackers on the same network can intercept your data. Use mobile data or a VPN for banking."
                },

                // ── QUESTION 9 — Multiple Choice ─────────────────
                new QuizQuestion
                {
                    Question = "What is SOCIAL ENGINEERING in cybersecurity?",
                    Options  = new List<string>
                    {
                        "A) Building social media apps",
                        "B) Tricking people psychologically into revealing confidential information",
                        "C) Managing a company's social media accounts",
                        "D) A type of antivirus software"
                    },
                    CorrectAnswer = "B",
                    Explanation   = "Social engineering manipulates human psychology — like pretending to be IT support to get your password — rather than hacking software."
                },

                // ── QUESTION 10 — True/False ─────────────────────
                new QuizQuestion
                {
                    Question      = "TRUE or FALSE: You should always hover over a link BEFORE clicking it to see where it actually goes.",
                    Options       = null,
                    CorrectAnswer = "True",
                    Explanation   = "Hovering reveals the real destination URL at the bottom of your browser. 'faceb00k.com' is NOT Facebook!"
                },

                // ── QUESTION 11 — Multiple Choice ────────────────
                new QuizQuestion
                {
                    Question = "In South Africa, where should you report a cybercrime?",
                    Options  = new List<string>
                    {
                        "A) report@cybersecurity.gov.za or SABRIC: 0861 022 339",
                        "B) Social media — post about it publicly",
                        "C) Just ignore it",
                        "D) Email the hacker directly"
                    },
                    CorrectAnswer = "A",
                    Explanation   = "In South Africa, report cybercrime to report@cybersecurity.gov.za, SABRIC (0861 022 339), or your local SAPS Cybercrime Unit."
                },

                // ── QUESTION 12 — True/False ─────────────────────
                new QuizQuestion
                {
                    Question      = "TRUE or FALSE: A shortened URL (like bit.ly/xyz) always shows you the safe, real destination before you click.",
                    Options       = null,
                    CorrectAnswer = "False",
                    Explanation   = "Shortened URLs HIDE the real destination. Use a URL expander tool (like checkshorturl.com) to see where a short link really goes."
                }
            };

            // Shuffle questions so the order is different each time
            ShuffleQuestions();
        }

        // ── SHUFFLE — Fisher-Yates algorithm ────────────────────
        private void ShuffleQuestions()
        {
            var rand = new Random();
            for (int i = _questions.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                var temp = _questions[i];
                _questions[i] = _questions[j];
                _questions[j] = temp;
            }
        }

        // ── START QUIZ ───────────────────────────────────────────
        // Resets everything and returns the first question as a string.
        public string StartQuiz()
        {
            _currentIndex = 0;
            _score = 0;
            _isActive = true;
            _awaitingAnswer = true;
            ShuffleQuestions();
            return GetCurrentQuestionText();
        }

        // ── GET CURRENT QUESTION TEXT ────────────────────────────
        // Formats the current question with options (or True/False).
        public string GetCurrentQuestionText()
        {
            if (_currentIndex >= _questions.Count)
                return GetFinalScore();

            QuizQuestion q = _questions[_currentIndex];
            var sb = new System.Text.StringBuilder();

            sb.AppendLine($"🧠 ═══ QUESTION {_currentIndex + 1} of {_questions.Count} ═══");
            sb.AppendLine();
            sb.AppendLine(q.Question);
            sb.AppendLine();

            if (q.IsMultipleChoice)
            {
                foreach (string opt in q.Options)
                    sb.AppendLine($"  {opt}");
                sb.AppendLine();
                sb.Append("Type A, B, C, or D:");
            }
            else
            {
                sb.AppendLine("  Type  True  or  False:");
            }

            return sb.ToString().TrimEnd();
        }

        // ── SUBMIT ANSWER ────────────────────────────────────────
        // Accepts the user's answer, checks it, returns feedback.
        // Returns null if the quiz is not active.
        public string SubmitAnswer(string userInput)
        {
            if (!_isActive || !_awaitingAnswer) return null;

            string answer = userInput.Trim().ToUpper();
            QuizQuestion q = _questions[_currentIndex];
            string correct = q.CorrectAnswer.ToUpper();

            // Normalise True/False input
            if (answer == "T") answer = "TRUE";
            if (answer == "F") answer = "FALSE";

            bool isCorrect = answer == correct.ToUpper();
            if (isCorrect) _score++;

            var sb = new System.Text.StringBuilder();

            if (isCorrect)
                sb.AppendLine("✅ CORRECT! Well done! 🎉");
            else
                sb.AppendLine($"❌ Not quite! The correct answer was: {q.CorrectAnswer}");

            sb.AppendLine();
            sb.AppendLine($"💡 Explanation: {q.Explanation}");
            sb.AppendLine();

            _currentIndex++;
            _awaitingAnswer = false;

            if (_currentIndex >= _questions.Count)
            {
                sb.AppendLine(GetFinalScore());
                _isActive = false;
            }
            else
            {
                sb.AppendLine("─────────────────────────────");
                sb.AppendLine(GetCurrentQuestionText());
                _awaitingAnswer = true;
            }

            return sb.ToString().TrimEnd();
        }

        // ── FINAL SCORE + FEEDBACK ───────────────────────────────
        private string GetFinalScore()
        {
            int pct = (_score * 100) / _questions.Count;
            string grade;

            if (pct == 100) grade = "🏆 PERFECT SCORE! You're a Cybersecurity Champion!";
            else if (pct >= 80) grade = "🌟 Great job! You're a cybersecurity pro!";
            else if (pct >= 60) grade = "👍 Good effort! Keep learning to stay safe online!";
            else if (pct >= 40) grade = "📚 Not bad! There's more to learn — keep it up!";
            else grade = "💪 Keep learning — every tip makes you safer online!";

            return $"🎯 ═══ QUIZ COMPLETE! ═══\n\n" +
                   $"Your score: {_score} / {_questions.Count}  ({pct}%)\n\n" +
                   grade +
                   "\n\nType 'start quiz' to play again, or ask me about any cybersecurity topic!";
        }
    }
}