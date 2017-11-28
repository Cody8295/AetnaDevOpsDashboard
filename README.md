# Aetna DevOps Dashboard

The Aetna DevOps Dashboard is a web GUI that displays the status of DevOps tools and processes in order to provide insights and simplify continous integration. By pulling information from various APIs, the DevOps Dashboard transforms the raw data into rich visualizations and interprets the data points in order to relate them to each other. This summary of the data provides management a useful perspective into the day-to-day business of software development at Aetna.

## Continous Integration
There are three fundamental parts to the continous integration cycle at Aetna:
* GitHub (for source code version control)

* TeamCity (for builds)

* Octopus (for deploys)

The responsibility of implementing Octopus into the dashboard was given to Team MMAC.

# Octopus 

Octopus is an [open-source](https://github.com/OctopusDeploy) application that automates deployment and release management for various software projects. At Aetna, they use it to deliver hundreds of applications over thousands of computers. Each project has a project group and each machine belongs to an environment. All of these details and their relationships to one another are exposed by Octopus' API.

## Octopus API

To provide the DevOps dashboard with the information it needs, the Octopus API is queried extensively using HTTP get calls. It would make testing a lot easier if the Octopus Client NuGet package was used instead, but the schedule did not allow for the replacement.  The Octopus API proves useful with dozens of endpoints to choose from, but we focused on these 8:

1. api/projectGroups
2. api/lifecycles
3. api/projects
4. api/environments
5. api/events
6. api/machines
7. api/progression
8. api/dashboard

### Project Groups

The project groups API call returns a list of the group of projects by name from Octopus. 

### Lifecycles

The lifecycles API returns the number of licecycles that exist on Octopus.

### Projects

The projects API returns a list of all projects.

### Environments

The environments API returns a list of all environments.

### Events

The events API is used to find the deployments that occured ovet the past 24 hours.

### Machines

The machines API returns a list of all machines and how many exist on each environment.

### Progression

The progression API is used to plot project releases on a timeline.

### Dashboard

The dashboard API is used to get the current deploys on each project by environment.


# Functionality

### View project groups and how many there are

### View all projects and search them by name

### View how many lifecycles there are

### View environments and how many there are

### View deploys that occured over past 24 hours (line graph, pie chart, bar graph)

### View additional details about deploys that happened over past 24 hours

### Filter deploy data by hour, category (succeeded, failed, queued, started)

### Navigate to Octopus web app for deploy on graphs.

### View timeline of releases by project

### View deploys of each release on timeline

### Navigate to Octopus web app for release on timeline.
