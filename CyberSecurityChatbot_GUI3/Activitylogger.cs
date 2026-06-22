using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberSecurityChatbot_GUI3
{
    // ============================================================
    // ACTIVITY LOGGER CLASS
    // Keeps a List<string> of every significant chatbot action.
    // Each entry includes a timestamp.
    // Only the last 10 entries are shown (rubric requirement).
    // The user types "show activity log" or "what have you done"
    // to view the log.
    // ============================================================
    public class ActivityLogger
    {
        // Internal list — stored in memory for the session
        // Each entry format: "[HH:mm:ss] description"
        private readonly List<string> _log = new List<string>();

        // Maximum entries to STORE before we trim the oldest
        private const int MaxStored = 50;

        // Maximum entries to DISPLAY at once (rubric: 5–10)
        private const int MaxDisplay = 10;

        // ── LOG AN ACTION ────────────────────────────────────────
        // Called by Chatbot.cs every time a significant action occurs.
        public void Log(string action)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            _log.Add($"[{timestamp}] {action}");

            // Keep stored list from growing forever
            if (_log.Count > MaxStored)
                _log.RemoveAt(0);
        }

        // ── GET LOG AS FORMATTED STRING ──────────────────────────
        // Returns the last MaxDisplay entries numbered and formatted.
        // This is displayed directly in the chat panel.
        public string GetFormattedLog(bool showAll = false)
        {
            if (_log.Count == 0)
                return "📋 No actions recorded yet. Start chatting, adding tasks, or playing the quiz!";

            int displayCount = showAll ? _log.Count : Math.Min(MaxDisplay, _log.Count);
            var recent = _log.Skip(_log.Count - displayCount).ToList();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("📋 ═══ ACTIVITY LOG ═══");
            sb.AppendLine($"Showing last {recent.Count} of {_log.Count} action(s):\n");

            for (int i = 0; i < recent.Count; i++)
                sb.AppendLine($"  {i + 1}. {recent[i]}");

            if (!showAll && _log.Count > MaxDisplay)
                sb.AppendLine($"\nType 'show full log' to see all {_log.Count} entries.");

            return sb.ToString().TrimEnd();
        }

        // ── TOTAL COUNT ──────────────────────────────────────────
        public int TotalCount => _log.Count;
    }
}