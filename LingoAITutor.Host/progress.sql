select Words.Text, UserWordProgresses.CorrectUses, UserWordProgresses.FailedToUseFlag,  
UserWordProgresses.NonUses, UserWordProgresses.EstimationExerciseNumber, UserWordProgresses.EstimationExerciseResult
from UserWordProgresses, Words where Words.Id = UserWordProgresses.WordID
order by text;