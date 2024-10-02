# Games and Artificial Intelligence Techniques<br>COSC2527 / COSC2528<br>Assignment 2
**Student ID:** s3721123

This is the README file for Assignment 2 in Games and Artificial Intelligence Techniques.

Notes:
* The starter project was created in Unity 2022.3.19f1. Please use the same Unity version for your submission.
* Please do not edit the contents of the .gitignore file.

## Tuning
I changed the computation budget from number of simulations to a time limit

All individual lines of results represents the winrate over 50 games

### Tuning c
- I tried c values above 0.5 but all lost (1.0, 0.8, 0.6)
- I then tried c=0.25 and it was very even, with c=0.5 winning slightly more often, so I figure perhaps they are opposite sides of a maximum
- With this in mind I tried a c value in between but slightly closer to 0.5 than 0.25
    - c=0.38 (52%) vs c=0.5 (42%)
- Next I set the untuned agent's c value to 0.38 and tried either side
    - c=0.41 (46%) vs c=0.38 (50%)
    - c=0.35 (40%) vs c=0.38 (58%)
    - c=0.39 (48%) vs c=0.38 (48%)
- The best c value seems to be roughly 0.38 

### Saving tree between moves
- Tuned MCTS winrate (c=0.38) : Untuned MCTS winrate (c=0.38)
    - 60% to 40%
- As expected this has made a noticable improvement to performance

### Gaussian distribution for rollout
- Using the Box-Muller method with a range of half a standard deviation (very slight preference for the middle) the tuned agent seemed to get worse
    - 36% to 60%
- This may be because calculating the random number this way takes too much of the computation budget
