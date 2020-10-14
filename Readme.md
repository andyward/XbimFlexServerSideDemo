# Xbim Flex Server-side demo

This is a demo application to show how you can work with and authentiate agains the Flex API server-to-server, rather than under the credentials of an end user.

Flex's resources are protected by OAuth2 to authenticate users. This provides a number of different 'flows' or grant types for different sorts of applications.

e.g. It supports SPA applications, native mobile applications, server side apps and 'unattended apps' using different flows.

This demo uses the Client Credentials flow so that the user of the Flex application is this software 'client'. 

## When to use this 'Client Credentials' approach

This approach might be appropriate if you just wanted to use Flex and a data processor (e.g. turn IFC files into data and wexbim Geometry on behalf of your system) 
- where you do not intend for end users to use the Flex collaboration features directly. 

It would also be appropriate for background processes, scheduled tasks etc, where there is no interactive user to grant the permission (and you don't want to worry about renewing tokens)

If you don't want to have to map users from an existing service to Flex users. (While this is simple and can be transparent through SSO it obviously requires some development)

## Things to be aware of

1. Your app will be entirely responsible for the access control and row level security of the data in your Flex Tenants
2. Because all activity is 'multiplexed' through a single user, the Flex audit trails will not ne useful
3. By routing all traffic through a second webserver there is a double handling of the data
4. All personalisation etc will be lost. Flex services that send messages/email will not work.

## Notes

As a demo app, some shortcuts have being taken for simplicity. You will want to employ some better engineering practices

1. Config such as secrets should not be in the code.
2. The state and caching system is very primitive
3. The app is hardwired to use the first tenant
4. Only a very slim cross section of the Flex API has been demonstrated.

## To Do

- [ ] Show how the viewer is integrated
- [ ] Hook up tenant switching


## Licence

Licenced under the MIT licence