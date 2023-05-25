# What is a reverse proxy?

Reverse proxies are used to route traffic to servers in a network. Or, from a Kubernetes perspective: A reverse proxy is used to route traffic to the correct Pod.

A reverse proxy is a machanism that prevents direct access to components in the network. Instead, it forwards it.

This article describes the following:

* How to expose a single webserver to the internet, directly
* How to prevent direct access to the webserver by implementing a Reverse Proxy
* How you can use a Reverse Proxy as a load balancer
* Typical reverse proxies

## Exposing a single webserver to the internet
When you have one webserver and a DNS domain, then that DNS domain usually points to the webserver, directly. To do so, you need one A-record:

`A: bff.gocloudnative.org 1.2.3.4`

When you configure your DNS this way, then the webserver is exposed to the internet directly. This situation is schematically displayed in the following diagram:

![Direct communication with an API](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/Concepts/diagrams/api.png)

## Preventing direct access by implementing a reverse proxy
If you want to prevent direct communication with the API, say, for security reasons, then the traffic must be relayed to it's destination. This is schematically displayed in the following diagram:

![Direct communication with an API](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/Concepts/diagrams/reverse-proxy-one-api.png)

In this situatation the A-record of your domain will point to the reverse proxy. This is the entry-point of your network. By implementing a reverse proxy, what traffic goes where or is allowed at all, can now be regulated by the reverse proxy.

## Using a reverse proxy as load balancer
Reverse Proxies are particulary usefull when you are scaling your application horizontally. In that case, you'll have multiple webservers with different IP addresses. Scaling will not be effective if the A-record always points to a single server. 

Instead, the A-record of your domain will point to the reverse proxy and it will decide how to devide the load between the two instances of the API. This is schematically displayed in the following diagram:

![Direct communication with an API](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/Concepts/diagrams/reverse-proxy-two-apis.png)

## A typical example of a Reverse Proxy in Kubernetes

A reverse proxy allows you to route traffic to other components in the network. It does not do so randomly. You must specify what traffic goes where.

A typical example of a reverse proxy is an Ingress Controller in Kubernetes. You define routing rules and Kubernetes forwards the request accordingly. This is what a typical ingress controller looks like:

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: minimal-ingress
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
spec:
  ingressClassName: nginx-example
  rules:
  - http:
      paths:
      - path: /testpath
        pathType: Prefix
        backend:
          service:
            name: test
            port:
              number: 80
```

You can also use a reverse proxy to augment requests to the destination.

## A typical example of a Load Balancing scenario with Yarp

Another example of a Reverse Proxy is Microsofts YARP (Yet Another Reverse Proxy). It works similarly to an Ingress controller: You define a route and you tell YARP where to forward the traffic to:

```json
{
 "ReverseProxy": {
   "Routes": {
     "route1": {
       "ClusterId": "cluster1",
       "Match": {
         "Path": "{**catch-all}"
       }
     }
   },
   "Clusters": {
     "cluster1": {
       "Destinations": {
         "cluster1/destination1": {
           "Address": "https://10.0.0.14/"
         },
         "cluster1/destination2": {
          "Address": "https://10.0.0.15/"
         }
       }
     }
   }
 }
}
```

In this example, traffic to all endpoints is forwarded to either `https://10.0.0.14` or `https://10.0.0.15`. You can extend the list of destinations endlessly. That's how you can use a Reverse Proxy as a load balancer.

## Summary
A reverse proxy is a mechanism that forwards traffic to other servers. You can use it to route traffic between your servers.