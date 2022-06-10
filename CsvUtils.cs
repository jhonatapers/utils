    public static class Csv
    {
        private static String[] SplitCsvLines(String csv, String lineSplitter)
        {
            List<String> csvLines = new List<String>();

            int aux = lineSplitter.Length / 2;
            int jump = lineSplitter.Length;
            int startPosition = 0;
            bool isInQuotes = false;
            for (int currentPosition = 0; currentPosition < csv.Length; currentPosition++)
            {
                if (csv.Length <= currentPosition + jump)
                    break;

                if (csv[currentPosition] == '\"')
                {
                    isInQuotes = !isInQuotes;
                }
                else if (csv.Substring(currentPosition, jump).Equals(lineSplitter) && !isInQuotes)
                {
                    csvLines.Add(csv.Substring(startPosition + aux, (currentPosition - aux) - startPosition));
                    startPosition = currentPosition + 1;
                }
            }


            return csvLines.ToArray();
        }
    }
