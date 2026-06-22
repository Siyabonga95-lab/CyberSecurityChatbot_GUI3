using System;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CyberSecurityChatbot_GUI3
{
    // ================================================================
    // MAINWINDOW CODE-BEHIND — Part 3 POE
    //
    // NEW IN PART 3 vs PART 2:
    //   - SidebarButton_Click handles Tasks, Quiz, Log sidebar buttons
    //   - UpdateSidebarBadges() keeps task/log counters live in sidebar
    //   - Input prompt dialogs for complete/delete (ID prompts)
    //   - All animations from Part 2 preserved (spinner + typing effect)
    // ================================================================
    public partial class MainWindow : Window
    {
        // ── FIELDS ────────────────────────────────────────────────
        private User _user;
        private Chatbot _chatbot;

        private DispatcherTimer _sessionTimer;
        private DispatcherTimer _spinnerTimer;
        private DispatcherTimer _typingTimer;
        private DispatcherTimer _badgeTimer;   // NEW: refreshes sidebar counters

        private bool _nameEntered = false;
        private bool _isBotBusy = false;

        // Typing animation state
        private string _fullResponseText = "";
        private int _typingIndex = 0;
        private TextBlock _typingTextBlock = null;

        // Spinner state
        private TextBlock _spinnerLabel = null;
        private Border _thinkingBubble = null;
        private int _spinnerFrame = 0;

        private static readonly string[] SpinnerFrames = { "◐", "◓", "◑", "◒" };

        // ================================================================
        // CONSTRUCTOR
        // ================================================================
        public MainWindow()
        {
            InitializeComponent();

            _chatbot = new Chatbot();
            _user = new User("");

            // Session clock
            _sessionTimer = new DispatcherTimer();
            _sessionTimer.Interval = TimeSpan.FromSeconds(1);
            _sessionTimer.Tick += SessionTimer_Tick;
            _sessionTimer.Start();

            // Spinner
            _spinnerTimer = new DispatcherTimer();
            _spinnerTimer.Interval = TimeSpan.FromMilliseconds(200);
            _spinnerTimer.Tick += SpinnerTimer_Tick;

            // Typing effect
            _typingTimer = new DispatcherTimer();
            _typingTimer.Interval = TimeSpan.FromMilliseconds(18); // slightly faster for longer responses
            _typingTimer.Tick += TypingTimer_Tick;

            // Badge refresh (every 2 seconds) — keeps task/log counters updated
            _badgeTimer = new DispatcherTimer();
            _badgeTimer.Interval = TimeSpan.FromSeconds(2);
            _badgeTimer.Tick += BadgeTimer_Tick;
            _badgeTimer.Start();

            PlayVoiceGreeting();
            ShowWelcomeMessage();
        }

        // ================================================================
        // VOICE GREETING
        // ================================================================
        private void PlayVoiceGreeting()
        {
            try
            {
                SoundPlayer player = new SoundPlayer(
                    @"C:\Users\Student\source\repos\CyberSecurityChatbot_1\CyberSecurityChatbot_1\Chatbot.wav");
                player.Play();
            }
            catch { /* Non-critical — continues if file not found */ }
        }

        // ================================================================
        // WELCOME MESSAGE (instant — no typing delay)
        // ================================================================
        private void ShowWelcomeMessage()
        {
            AddBotMessageInstant(
                "╔════════════════════════════════════════╗\n" +
                "║     SIYA'S CYBER CHATBOT  — PART 3    ║\n" +
                "║     Stay Safe Online! 🛡️               ║\n" +
                "╚════════════════════════════════════════╝\n\n" +
                "NEW FEATURES:\n" +
                "  📝 Task Assistant  — Add & manage cybersecurity tasks\n" +
                "  🧠 Quiz            — Test your cybersecurity knowledge\n" +
                "  📋 Activity Log   — See what the chatbot has done\n" +
                "  🤖 Smarter NLP    — Understands natural language better\n\n" +
                "Use the SIDEBAR buttons on the left, or just type naturally!\n\n" +
                "Before we start — what's your name?"
            );
        }

        // ================================================================
        // TIMER TICKS
        // ================================================================
        private void SessionTimer_Tick(object sender, EventArgs e)
        {
            if (_nameEntered)
            {
                txtSessionTime.Text = $"⏱  Session: {_user.GetSessionTime()}";
                txtQuestionCount.Text = $"💬 Messages: {_user.QuestionCount}";
                if (!string.IsNullOrEmpty(_user.FavouriteTopic))
                    txtMemoryIndicator.Text = $"🧠 Remembering: {_user.FavouriteTopic}";
            }
        }

        // Updates the sidebar badges (task count + log count)
        private void BadgeTimer_Tick(object sender, EventArgs e)
        {
            UpdateSidebarBadges();
        }

        private void UpdateSidebarBadges()
        {
            // We only update after name is entered (chatbot is initialised)
            if (!_nameEntered) return;
            try
            {
                // We need a DatabaseManager to count tasks — use the one inside Chatbot
                // The easiest way: just update the log count from ActivityLogger
                txtLogBadge.Text = $"Log entries: {_chatbot.Logger.TotalCount}";
            }
            catch { }
        }

        private void SpinnerTimer_Tick(object sender, EventArgs e)
        {
            _spinnerFrame = (_spinnerFrame + 1) % SpinnerFrames.Length;
            if (_spinnerLabel != null)
                _spinnerLabel.Text = $"{SpinnerFrames[_spinnerFrame]}  Bot is thinking...";
        }

        private void TypingTimer_Tick(object sender, EventArgs e)
        {
            if (_typingIndex < _fullResponseText.Length)
            {
                _typingIndex++;
                _typingTextBlock.Text = _fullResponseText.Substring(0, _typingIndex);
                ScrollToBottom();
            }
            else
            {
                _typingTimer.Stop();
                _isBotBusy = false;
                txtInput.IsEnabled = true;
                btnSend.IsEnabled = true;
                txtInput.Focus();
                ScrollToBottom();
                UpdateSidebarBadges();
            }
        }

        // ================================================================
        // BUTTON & KEYBOARD HANDLERS
        // ================================================================
        private void btnSend_Click(object sender, RoutedEventArgs e) => ProcessInput();

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ProcessInput();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            _spinnerTimer.Stop();
            _typingTimer.Stop();
            _isBotBusy = false;
            ChatPanel.Children.Clear();
            EnableInput();
            AddBotMessageInstant("Chat cleared! 🗑️  How can I help you, " +
                                 (_nameEntered ? _user.Name : "friend") + "?");
        }

        // ── TOPIC QUICK BUTTONS (Part 2 preserved) ───────────────
        private void TopicButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            string query = "";
            if (btn == btnPhishing) query = "Give me a phishing tip";
            if (btn == btnPasswords) query = "Give me a password tip";
            if (btn == btnPrivacy) query = "Give me a privacy tip";
            if (btn == btnScams) query = "Give me a scam tip";
            if (btn == btnViruses) query = "Give me a virus tip";
            if (btn == btnLinks) query = "How to spot fake links?";
            if (btn == btnMenu) query = "menu";

            if (!string.IsNullOrEmpty(query))
            {
                txtInput.Text = query;
                ProcessInput();
            }
        }

        // ── SIDEBAR BUTTONS — Part 3 Feature Buttons ─────────────
        // Each sidebar button injects a natural-language command into the input
        // so the NLPProcessor and Chatbot handle all the logic.
        private void SidebarButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isBotBusy || !_nameEntered) return;

            Button btn = sender as Button;
            if (btn == null) return;

            string query = "";

            if (btn == btnAddTask)
            {
                // Prompt the user to type what they want to add
                txtInput.Text = "add task ";
                txtInput.Focus();
                txtInput.SelectionStart = txtInput.Text.Length;
                txtInput.CaretIndex = txtInput.Text.Length;
                txtStatus.Text = "💡 Type the task name after 'add task' and press Enter. E.g: add task Enable 2FA";
                return; // Don't send yet — let user complete it
            }

            if (btn == btnViewTasks) query = "show tasks";
            if (btn == btnCompleteSidebar)
            {
                txtInput.Text = "complete task ";
                txtInput.Focus();
                txtInput.CaretIndex = txtInput.Text.Length;
                txtStatus.Text = "💡 Type the Task ID after 'complete task'. E.g: complete task 1";
                return;
            }
            if (btn == btnDeleteSidebar)
            {
                txtInput.Text = "delete task ";
                txtInput.Focus();
                txtInput.CaretIndex = txtInput.Text.Length;
                txtStatus.Text = "💡 Type the Task ID after 'delete task'. E.g: delete task 2";
                return;
            }
            if (btn == btnStartQuiz) query = "start quiz";
            if (btn == btnShowLog) query = "show activity log";
            if (btn == btnShowFullLog) query = "show full log";
            if (btn == btnMenuSidebar) query = "menu";

            if (!string.IsNullOrEmpty(query))
            {
                txtInput.Text = query;
                ProcessInput();
            }
        }

        // ================================================================
        // PROCESS INPUT — the heart of the chatbot interaction
        // (Identical flow to Part 2 + NLP/task routing in Chatbot.cs)
        // ================================================================
        private async void ProcessInput()
        {
            if (_isBotBusy) return;

            string input = txtInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                txtStatus.Text = "⚠️  Please type a message before sending!";
                return;
            }

            txtStatus.Text = "💡 Tip: Use sidebar buttons or type 'menu' for all options";

            AddUserMessage(input);
            txtInput.Clear();

            // ── NAME COLLECTION (first message only) ──
            if (!_nameEntered)
            {
                if (!IsValidName(input))
                {
                    AddBotMessageInstant(
                        "Please enter a real name using only letters — no numbers or symbols! 😊\n\nWhat's your name?");
                    return;
                }

                string cleanName = CapitaliseName(input);
                _user = new User(cleanName);
                _nameEntered = true;

                txtSessionTime.Text = $"⏱  Session: {_user.GetSessionTime()}";
                txtQuestionCount.Text = "💬 Messages: 0";

                await ShowBotResponseAnimated(
                    $"Hello {_user.Name}! 👋 Welcome to the Part 3 Enhanced Cybersecurity Chatbot!\n\n" +
                    "I have 4 powerful new features:\n\n" +
                    "📝 TASK ASSISTANT — Click '➕ Add Task' or type 'add task Enable 2FA'\n" +
                    "🧠 QUIZ           — Click '🎮 Start Quiz' or type 'start quiz'\n" +
                    "📋 ACTIVITY LOG   — Click '📋 Activity Log' or type 'show activity log'\n" +
                    "🤖 SMART NLP      — I understand natural phrases like 'remind me to update my password'\n\n" +
                    "I still know everything from Parts 1 and 2!\n" +
                    "Type 'menu' for the full list of everything I can do. 😊"
                );
                return;
            }

            // ── EXIT ──
            if (input.ToLower() == "exit" || input.ToLower() == "quit" || input.ToLower() == "bye")
            {
                await ShowBotResponseAnimated(
                    $"Goodbye {_user.Name}! 👋 Stay safe online! 🛡️\n\n" +
                    $"Session Summary:\n" +
                    $"⏱  Time: {_user.GetSessionTime()}\n" +
                    $"💬 Messages: {_user.QuestionCount}\n" +
                    (!string.IsNullOrEmpty(_user.FavouriteTopic) ?
                        $"🧠 Favourite topic: {_user.FavouriteTopic}\n" : "") +
                    $"📋 Actions logged: {_chatbot.Logger.TotalCount}\n\n" +
                    "Come back anytime! Knowledge is your best defence. 🔐"
                );
                txtInput.IsEnabled = false;
                btnSend.IsEnabled = false;
                _sessionTimer.Stop();
                _badgeTimer.Stop();
                return;
            }

            // ── NORMAL MESSAGE ──
            _user.IncrementQuestions();
            string response = _chatbot.GetResponse(input, _user);
            await ShowBotResponseAnimated(response);
        }

        // ================================================================
        // ANIMATION METHODS (unchanged from Part 2)
        // ================================================================
        private async Task ShowBotResponseAnimated(string response)
        {
            _isBotBusy = true;
            txtInput.IsEnabled = false;
            btnSend.IsEnabled = false;

            ShowThinkingBubble();
            await Task.Delay(2500); // slightly shorter wait for better UX
            RemoveThinkingBubble();
            StartTypingResponse(response);
        }

        private void ShowThinkingBubble()
        {
            _thinkingBubble = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#161B22")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C8FF")),
                BorderThickness = new Thickness(3, 0, 0, 0),
                CornerRadius = new CornerRadius(0, 8, 8, 8),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(0, 6, 80, 6),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 400
            };

            StackPanel content = new StackPanel();

            TextBlock botLabel = new TextBlock
            {
                Text = "🤖  Bot",
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C8FF")),
                Margin = new Thickness(0, 0, 0, 4)
            };

            _spinnerLabel = new TextBlock
            {
                Text = "◐  Bot is thinking...",
                FontFamily = new FontFamily("Consolas"),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B949E"))
            };

            content.Children.Add(botLabel);
            content.Children.Add(_spinnerLabel);
            _thinkingBubble.Child = content;

            ChatPanel.Children.Add(_thinkingBubble);
            ScrollToBottom();

            _spinnerFrame = 0;
            _spinnerTimer.Start();
        }

        private void RemoveThinkingBubble()
        {
            _spinnerTimer.Stop();
            if (_thinkingBubble != null && ChatPanel.Children.Contains(_thinkingBubble))
                ChatPanel.Children.Remove(_thinkingBubble);
            _thinkingBubble = null;
            _spinnerLabel = null;
        }

        private void StartTypingResponse(string response)
        {
            _fullResponseText = response;
            _typingIndex = 0;

            Border bubble = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#161B22")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C8FF")),
                BorderThickness = new Thickness(3, 0, 0, 0),
                CornerRadius = new CornerRadius(0, 8, 8, 8),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(0, 6, 80, 6),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 720
            };

            StackPanel content = new StackPanel();

            TextBlock label = new TextBlock
            {
                Text = "🤖  Bot",
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C8FF")),
                Margin = new Thickness(0, 0, 0, 4)
            };

            _typingTextBlock = new TextBlock
            {
                Text = "",
                FontFamily = new FontFamily("Consolas"),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E6EDF3")),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };

            content.Children.Add(label);
            content.Children.Add(_typingTextBlock);
            bubble.Child = content;

            ChatPanel.Children.Add(bubble);
            _typingTimer.Start();
        }

        private void AddBotMessageInstant(string message)
        {
            Border bubble = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#161B22")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C8FF")),
                BorderThickness = new Thickness(3, 0, 0, 0),
                CornerRadius = new CornerRadius(0, 8, 8, 8),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(0, 6, 80, 6),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 720
            };

            StackPanel content = new StackPanel();

            TextBlock label = new TextBlock
            {
                Text = "🤖  Bot",
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C8FF")),
                Margin = new Thickness(0, 0, 0, 4)
            };

            TextBlock text = new TextBlock
            {
                Text = message,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E6EDF3")),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };

            content.Children.Add(label);
            content.Children.Add(text);
            bubble.Child = content;

            ChatPanel.Children.Add(bubble);
            ScrollToBottom();
        }

        private void AddUserMessage(string message)
        {
            string displayName = _nameEntered ? _user.Name : "You";

            Border bubble = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#21262D")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7B72")),
                BorderThickness = new Thickness(0, 0, 3, 0),
                CornerRadius = new CornerRadius(8, 0, 8, 8),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(80, 6, 0, 6),
                HorizontalAlignment = HorizontalAlignment.Right,
                MaxWidth = 720
            };

            StackPanel content = new StackPanel();

            TextBlock label = new TextBlock
            {
                Text = $"👤  {displayName}",
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7B72")),
                Margin = new Thickness(0, 0, 0, 4),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            TextBlock text = new TextBlock
            {
                Text = message,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E6EDF3")),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            content.Children.Add(label);
            content.Children.Add(text);
            bubble.Child = content;

            ChatPanel.Children.Add(bubble);
            ScrollToBottom();
        }

        // ================================================================
        // HELPERS
        // ================================================================
        private void EnableInput()
        {
            _isBotBusy = false;
            txtInput.IsEnabled = true;
            btnSend.IsEnabled = true;
            txtInput.Focus();
        }

        private void ScrollToBottom()
        {
            ChatScrollViewer.UpdateLayout();
            ChatScrollViewer.ScrollToEnd();
        }

        private bool IsValidName(string name)
        {
            name = name.Trim();
            if (string.IsNullOrWhiteSpace(name)) return false;
            foreach (char c in name)
                if (!char.IsLetter(c) && c != ' ') return false;
            return true;
        }

        private string CapitaliseName(string name)
        {
            string[] words = name.Trim().Split(' ');
            for (int i = 0; i < words.Length; i++)
                if (words[i].Length > 0)
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            return string.Join(" ", words);
        }
    }
}