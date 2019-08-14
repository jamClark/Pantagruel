# Pantagruel
A general-purpose XML serializer for C# applications with special-case handling for use with Unity3D.

A robust serializer/deserializer that can be used to convert complex objects graphs to and from XML text streams. It was primarily designed with Unity3D's unique objects in mind and thus supports many additional functionalities for handling GameObject hierarchies, Components, and resources-base references. It handles primitives, classes, object references, circular references, polymorphic datatypes, collections, generic types, delegates, and many common built-in Unity objects and resources as well.

This serializer was designed to be very flexible and highly extensible through the use of the ISerializationSurrogate and IActivator interfaces. This allows for arbitrarily complex data to be handled in any situation.

Issues: This was originall written during the Unity 5.2 release cycle and likely has several serialization surrogates that are out of date with more modern formats for built-in Unity resource and component types. The appropriate surrogates will likely need to be tested, checked for errors, and modified as required.
