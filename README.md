[![Build Status](https://johnmalc.visualstudio.com/MeetupToRTM/_apis/build/status/Main%20Pipeline?branchName=master)](https://johnmalc.visualstudio.com/MeetupToRTM/_build/latest?definitionId=6&branchName=master)
[![Sonarcloud Status](https://sonarcloud.io/api/project_badges/measure?project=dmpe_MeetupToRTM&metric=alert_status)](https://sonarcloud.io/dashboard?id=dmpe_MeetupToRTM) 
[![SonarCloud Bugs](https://sonarcloud.io/api/project_badges/measure?project=dmpe_MeetupToRTM&metric=bugs)](https://sonarcloud.io/component_measures/metric/reliability_rating/list?id=dmpe_MeetupToRTM)
[![SonarCloud Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=dmpe_MeetupToRTM&metric=vulnerabilities)](https://sonarcloud.io/component_measures/metric/security_rating/list?id=dmpe_MeetupToRTM)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=dmpe_MeetupToRTM&metric=code_smells)](https://sonarcloud.io/dashboard?id=dmpe_MeetupToRTM)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=dmpe_MeetupToRTM&metric=coverage)](https://sonarcloud.io/dashboard?id=dmpe_MeetupToRTM)
![GitHub](https://img.shields.io/github/license/dmpe/MeetupToRTM.svg)
 
# MeetupToRTM: C# .NetFramework 4.7 Desktop Application

## Objective:
 - Export your upcoming Meetup.com events into Remember The Milk tasks.
   - Thus replacing a paid functionality available through [IFTTT](https://www.rememberthemilk.com/services/ifttt/)

### Features/TODO:
 - Fetches your [meetup events](https://www.meetup.com/meetup_api/docs/self/events/) and converts them to tasks. 
   The format used, by default and which can be also adjusted, is <ID-MeetupRTM: _meetupEventID_ _MeetupName_>. 
 
 - [ ] In case of a frequent use, the app can recognize already added tasks (through abovementioned format) and will skip adding them again. 
       Therefore de-cluttering your list and avoding duplicates. :yum:

 - [ ] For testing, use secret keys <https://stackoverflow.com/questions/50299066/how-to-pass-secrets-into-the-test-execution-context-from-vsts>

#### Requirenments to making it work:

You will need to have 2 API keys:

 - API Key from Meetup <https://secure.meetup.com/meetup_api/key/>
 - API Keys from RTM <https://www.rememberthemilk.com/services/api/>

![image](images/rtm_meetup.PNG)
![image2](images/rtm_smartlist.PNG)

# Building/Testing MeetupToRTM

I use Azure Pipelines for CI/CD. See here <https://johnmalc.visualstudio.com/MeetupToRTM/_build/>

How to update submodule to point to the latest changes?

```
cd MeetupToRTM
git submodule update --remote 
```

## How to start

Download from release page a `zip` archive and unpack it somewhere. Double click on exe file -> app should start automatically.

**Class Diagramm anybody ? Sure here we go:**

![image3](images/ClassDiagram_MeetupToRTM.png)


# Achievements

In order to make this application work, I had to fork and submit patches to the <https://github.com/dmpe/RememberTheMilkApi>, see `merged_branch_with_all_changes` branch. 
Indeed, this programm depends on the upstream RTM library.
