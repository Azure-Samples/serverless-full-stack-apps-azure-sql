name: Deploy DotNet project to function app with a Linux environment

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

env:
      AZURE_FUNCTIONAPP_PACKAGE_PATH: 'azure-function/dotnet' # set this to the path to your web app project, defaults to the repository root
      AZURE_FUNCTIONAPP_NAME: antho-catch-bus # set this to your application's name
      DOTNET_VERSION: '6.0'

jobs:
  build-and-deploy-function-app:
    runs-on: ubuntu-latest
    steps:
    - name: 'Checkout GitHub Action'
      uses: actions/checkout@main

    - name: Setup DotNet ${{ env.DOTNET_VERSION }} Environment
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}

    - name: 'Resolve Project Dependencies Using Dotnet'
      shell: bash
      run: |
        pushd './${{ env.AZURE_FUNCTIONAPP_PACKAGE_PATH }}'
        dotnet build --configuration Release --output ./output
        popd
    - name: 'Run Azure Functions Action'
      uses: Azure/functions-action@v1
      id: fa
      with:
        app-name: ${{ env.AZURE_FUNCTIONAPP_NAME }}
        package: '${{ env.AZURE_FUNCTIONAPP_PACKAGE_PATH }}/output'
        publish-profile: ${{ secrets.AZURE_FUNCTIONAPP_PUBLISH_PROFILE }}
      
