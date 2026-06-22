using System;

namespace CyberSecurityChatbot_GUI3
{
    // ============================================================
    // USER CLASS — unchanged from Part 2
    // Stores everything the chatbot needs to remember about the user.
    // ============================================================
    public class User
    {
        public string Name { get; set; }
        public bool IsExiting { get; set; }
        public DateTime StartTime { get; set; }
        public int QuestionCount { get; set; }
        public string FavouriteTopic { get; set; }
        public string LastTopic { get; set; }
        public System.Collections.Generic.Dictionary<string, int> TopicMentionCount { get; set; }

        public User(string name)
        {
            Name = name;
            IsExiting = false;
            StartTime = DateTime.Now;
            QuestionCount = 0;
            FavouriteTopic = "";
            LastTopic = "";
            TopicMentionCount = new System.Collections.Generic.Dictionary<string, int>();
        }

        public void IncrementQuestions()
        {
            QuestionCount++;
        }

        public void RecordTopicMention(string topic)
        {
            if (string.IsNullOrEmpty(topic)) return;
            if (TopicMentionCount.ContainsKey(topic))
                TopicMentionCount[topic]++;
            else
                TopicMentionCount[topic] = 1;

            if (TopicMentionCount[topic] >= 2 && string.IsNullOrEmpty(FavouriteTopic))
                FavouriteTopic = topic;
        }

        public void SetFavouriteTopic(string topic)
        {
            if (!string.IsNullOrEmpty(topic))
            {
                FavouriteTopic = topic;
                TopicMentionCount[topic] = 5;
            }
        }

        public string GetSessionTime()
        {
            TimeSpan time = DateTime.Now - StartTime;
            return $"{time.Minutes} min, {time.Seconds} sec";
        }
    }
}