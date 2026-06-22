using System;

namespace CyberSecurityChatbot_GUI3
{
    // ============================================================
    // TASK ITEM — Represents one cybersecurity task stored in SQLite
    // ============================================================
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Reminder { get; set; }   // e.g. "3 days" or a date string
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }

        public TaskItem()
        {
            CreatedAt = DateTime.Now;
            IsCompleted = false;
            Reminder = "";
        }

        public TaskItem(string title, string description, string reminder = "")
        {
            Title = title;
            Description = description;
            Reminder = reminder;
            IsCompleted = false;
            CreatedAt = DateTime.Now;
        }

        // Used in the chat panel to display each task nicely
        public override string ToString()
        {
            string status = IsCompleted ? "✅" : "🔲";
            string reminderText = string.IsNullOrEmpty(Reminder)
                                  ? ""
                                  : $"  ⏰ {Reminder}";
            return $"{status} [ID:{Id}] {Title}{reminderText}\n     └─ {Description}";
        }
    }
}