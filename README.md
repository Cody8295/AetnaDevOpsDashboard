# Aetna DevOps Dashboard

A dashboard web UI designed to see the overall status of DevOps tools and processes, this application enables the simplification of continous integration. By pulling information from various API's, the DevOps Dashboard cleverly transforms the datapoints into rich visuals and useful chunks of information which meaningfully relate to each other. This summary of the data provides management a very useful perspective into the day-to-day business of software development at Aetna.

##Continous Integration
There are three fundamental parts to the continous integration cycle at Aetna:
*GitHub (for source code version control)
*TeamCity (for builds)
*Octopus (for deploys)

The responsibility of implementing Octopus into the dashboard was given to Team MMAC.

# Octopus 

Octopus is an [open-source](https://github.com/OctopusDeploy) application that automates deployment and release management for various software projects. At Aetna, they use it to deliver hundreds of applications over thousands of computers. Each project has a project group and each machine belongs to an environment. All of these details and their relationships to one another are exposed by Octopus' API.

##Octopus API

To provide the DevOps dashboard with the information it needs, the Octopus API is queried extensively using HTTP get calls. It would make testing a lot easier if the Octopus Client NuGet package was used instead, but the schedule did not allow for the replacement.  The Octopus API proves useful with dozens of endpoints to choose from, but we focused on these 8:

1. /projectGroups
2. /lifecycles
3. /projects
4. /environments
5. /events
6. /machines
7. /progression
8. /dashboard

