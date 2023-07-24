# Automatic WebRessource publisher

awrp is a tool to upload PowerApps WebResources dynamically to a solution.
Initialize a new connection for your password using your host (organization>.crm.dynamics.com), a user, and his user secret.
Adding files using the 'add' command and push them dynamically to your existing solution.

# Manual
```text
Usage: awrp.exe [init|add|push|remove|help] [options]


DESCRIPTION
    awrp is a tool to upload PowerApps WebResources dynamically to a solution.
    Initialize a new connection for your password using your host (organization>.crm.dynamics.com), a user, and his user secret.
    Adding files using the 'add' command and push them dynamically to your existing solution.

COMMANDS
    init
        Initialize a new connection in the current working directory

    add
        Add a file to the tracking list
            -f <file>
            The file relative to the config file that should be uploaded
            -s <solution>
            The Solutions unique name
            [-p <prefix>]
            When the resource path prefix is not set, the default of the config file is used
            [-d <description>]
            The description of the WebResource

    remove
        Removes a file from the tracking list

        OPTIONS
            -f <file>
            The file relative to the config file that should be uploaded

        Upload all tracked files to your environment

    help
        Prints this text
```