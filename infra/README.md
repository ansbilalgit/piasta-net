### Azure environment deployment templates

#### Login to Azure:
`az login`

#### Check the modifications:
`az deployment group what-if --resource-group gu_adp_piasta-net --template-file azuredeploy.json --parameters azuredeploy.parameters.json`   

#### Deploy the template:
`az deployment group create --resource-group gu_adp_piasta-net --template-file azuredeploy.json --parameters azuredeploy.parameters.json`   

It will ask for the slqadmin password:  
`Please provide securestring value for 'sqlAdministratorPassword' (? for help):`   
Paste it (from the Azure Key-Vault) and press enter.   
Then wait...   
drink a coffee...   
and/or pray. :)   
