select Words.Text, UserWordProgresses.CorrectUses, UserWordProgresses.FailedToUseFlag,  
UserWordProgresses.NonUses, UserWordProgresses.EstimationExerciseNumber, UserWordProgresses.EstimationExerciseResult,
Words.FrequencyRank r
from UserWordProgresses, Words where Words.Id = UserWordProgresses.WordID AND UserWordProgresses.FailedToUseFlag = 1
order by r;