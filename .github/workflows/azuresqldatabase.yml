name: 'Deploy Azure SQL Database schema'

on:
  push:
    branches:
      - main
  pull_request:
    types: [opened, synchronize, reopened, closed]
    branches:
      - main

jobs:
  deploy_database_job:
    if: github.event_name == 'push' || (github.event_name == 'pull_request' && github.event.action != 'closed')
    runs-on: ubuntu-latest
    name: Deploy Database Job
    steps:
      - uses: actions/checkout@v2
      - uses: Azure/sql-action@v1.2
        with:
          connection-string: ${{ secrets.AZURE_SQL_CONNECTION_STRING }}
          # uses dacpac to deploy database schema
          dacpac-package: './database/dacpac/bus-db.dacpac'