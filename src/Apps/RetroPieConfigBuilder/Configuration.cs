using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RetroPieConfigBuilder
{
    public class Configuration
    {
        public string Input_player1_a_btn { get; set; }

        public string Input_player1_b_btn { get; set; }

        public string Input_player1_y_btn { get; set; }

        public string Input_player1_x_btn { get; set; }

        public string Input_player1_l_btn { get; set; }

        public string Input_player1_r_btn { get; set; }

        public string Input_player1_l2_btn { get; set; }

        public string Input_player1_r2_btn { get; set; }

        public string Input_player2_a_btn { get; set; }

        public string Input_player2_b_btn { get; set; }

        public string Input_player2_y_btn { get; set; }

        public string Input_player2_x_btn { get; set; }

        public string Input_player2_l_btn { get; set; }

        public string Input_player2_r_btn { get; set; }

        public string Input_player2_l2_btn { get; set; }

        public string Input_player2_r2_btn { get; set; }

        public static Configuration ParseFile(
            string filePath)
        {
            var contents = File.ReadAllText(filePath);
            var config = new Configuration()
            {
                Input_player1_a_btn = contents.FindFirstBetween("input_player1_a_btn = \"", "\""),
                Input_player1_b_btn = contents.FindFirstBetween("input_player1_b_btn = \"", "\""),
                Input_player1_y_btn = contents.FindFirstBetween("input_player1_y_btn = \"", "\""),
                Input_player1_x_btn = contents.FindFirstBetween("input_player1_x_btn = \"", "\""),
                Input_player1_l_btn = contents.FindFirstBetween("input_player1_l_btn = \"", "\""),
                Input_player1_r_btn = contents.FindFirstBetween("input_player1_r_btn = \"", "\""),
                Input_player1_l2_btn = contents.FindFirstBetween("input_player1_l2_btn = \"", "\""),
                Input_player1_r2_btn = contents.FindFirstBetween("input_player1_r2_btn = \"", "\""),
                Input_player2_a_btn = contents.FindFirstBetween("input_player2_a_btn = \"", "\""),
                Input_player2_b_btn = contents.FindFirstBetween("input_player2_b_btn = \"", "\""),
                Input_player2_y_btn = contents.FindFirstBetween("input_player2_y_btn = \"", "\""),
                Input_player2_x_btn = contents.FindFirstBetween("input_player2_x_btn = \"", "\""),
                Input_player2_l_btn = contents.FindFirstBetween("input_player2_l_btn = \"", "\""),
                Input_player2_r_btn = contents.FindFirstBetween("input_player2_r_btn = \"", "\""),
                Input_player2_l2_btn = contents.FindFirstBetween("input_player2_l2_btn = \"", "\""),
                Input_player2_r2_btn = contents.FindFirstBetween("input_player2_r2_btn = \"", "\""),
            };

            return config;
        }

        public void Write(
            string templateFilePath,
            string outputFilePath)
        {
            var contents = File.ReadAllText(templateFilePath);
            contents = contents
                .Replace("A", this.Input_player1_a_btn)
                .Replace("B", this.Input_player1_b_btn)
                .Replace("Y", this.Input_player1_y_btn)
                .Replace("X", this.Input_player1_x_btn)
                .Replace("L1", this.Input_player1_l_btn)
                .Replace("R1", this.Input_player1_r_btn)
                .Replace("L2", this.Input_player1_l2_btn)
                .Replace("R2", this.Input_player1_r2_btn);

            File.WriteAllText(outputFilePath, contents);
        }
    }
}
