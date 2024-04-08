select "Words"."Text", "UserWordProgresses"."CorrectUses", "UserWordProgresses"."FailedToUseFlag",  
"UserWordProgresses"."NonUses", "UserWordProgresses"."EstimationExerciseNumber", "UserWordProgresses"."EstimationExerciseResult",
"Words"."FrequencyRank" r, "UserWordProgresses"."FailedToUseSencence" 
from "UserWordProgresses", "Words" where "Words"."Id" = "UserWordProgresses"."WordID" AND "UserWordProgresses"."FailedToUseFlag" = true
order by r