# Implementing the BFF Security Pattern in Azure Container Apps

In many cases, it is desirable to use a BFF (Backend For Frontend) as an Authentication Gateway when you have a microservices architecture. Microservices are often hosted on a Kubernetes-based platform, like Azure Container.

However, since Microsoft Azure Container Apps manages is in essence a managed Kubernetes Cluster, it does not provide the same level of flexibility as Kubernetes itself. Therefore, if you are using Azure Container Apps, you need to approach things slightly differently.

In this article, you will learn how to set up a microservices landscape using Azure Container Apps and the GoCloudNative.BFF.

# Architecture

Assuming that the application will scale horizontally, it is likely that the following architecture will emerge:

![GoCloudNative.Bff authentication flow](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/Diagrams/azure-container-apps-demo.png)
