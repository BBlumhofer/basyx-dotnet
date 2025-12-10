/*******************************************************************************
* Copyright (c) 2024 Bosch Rexroth AG
* Author: Constantin Ziesche (constantin.ziesche@bosch.com)
*
* This program and the accompanying materials are made available under the
* terms of the MIT License which is available at
* https://github.com/eclipse-basyx/basyx-dotnet/blob/main/LICENSE
*
* SPDX-License-Identifier: MIT
*******************************************************************************/
using BaSyx.Models.AdminShell;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BaSyx.Models.Extensions
{
    public class ReferenceJsonConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert == null)
            {
                return false;
            }

            if (typeof(IReference).IsAssignableFrom(typeToConvert))
            {
                return true;
            }

            return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(IReference<>);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(IReference<>))
            {
                var referableType = typeToConvert.GetGenericArguments()[0];
                var converterType = typeof(GenericReferenceConverter<>).MakeGenericType(referableType);
                return (JsonConverter)Activator.CreateInstance(converterType);
            }

            return new NonGenericReferenceConverter();
        }

        private static ReferencePayload ReadPayload(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return ReferencePayload.Empty;
            }

            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            JsonElement root = document.RootElement;

            ReferenceType referenceType = ReferenceType.Undefined;
            if (root.TryGetProperty("type", out JsonElement typeElement))
            {
                if (typeElement.ValueKind == JsonValueKind.String && Enum.TryParse(typeElement.GetString(), true, out ReferenceType parsedType))
                {
                    referenceType = parsedType;
                }
                else if (typeElement.ValueKind == JsonValueKind.Number && typeElement.TryGetInt32(out int enumValue))
                {
                    referenceType = (ReferenceType)enumValue;
                }
            }

            var keys = new List<IKey>();
            if (root.TryGetProperty("keys", out JsonElement keysElement) && keysElement.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement keyElement in keysElement.EnumerateArray())
                {
                    if (keyElement.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    KeyType keyType = KeyType.Undefined;
                    string value = null;

                    if (keyElement.TryGetProperty("type", out JsonElement keyTypeElement))
                    {
                        if (keyTypeElement.ValueKind == JsonValueKind.String && Enum.TryParse(keyTypeElement.GetString(), true, out KeyType parsedKeyType))
                        {
                            keyType = parsedKeyType;
                        }
                        else if (keyTypeElement.ValueKind == JsonValueKind.Number && keyTypeElement.TryGetInt32(out int keyTypeInt))
                        {
                            keyType = (KeyType)keyTypeInt;
                        }
                    }

                    if (keyElement.TryGetProperty("value", out JsonElement valueElement) && valueElement.ValueKind == JsonValueKind.String)
                    {
                        value = valueElement.GetString();
                    }

                    if (!string.IsNullOrEmpty(value))
                    {
                        keys.Add(new Key(keyType, value));
                    }
                }
            }

            IReference referredSemanticId = null;
            if (root.TryGetProperty("referredSemanticId", out JsonElement semanticElement) && semanticElement.ValueKind != JsonValueKind.Null)
            {
                referredSemanticId = semanticElement.Deserialize<IReference>(options);
            }

            return new ReferencePayload(referenceType, keys.ToArray(), referredSemanticId);
        }

        private static void WriteReference(Utf8JsonWriter writer, IReference value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            writer.WriteString("type", value.Type.ToString());

            if (value.Keys != null)
            {
                writer.WritePropertyName("keys");
                writer.WriteStartArray();

                foreach (IKey key in value.Keys)
                {
                    if (key == null)
                    {
                        continue;
                    }

                    writer.WriteStartObject();
                    writer.WriteString("type", key.Type.ToString());
                    if (!string.IsNullOrEmpty(key.Value))
                    {
                        writer.WriteString("value", key.Value);
                    }
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }

            if (value.ReferredSemanticId != null)
            {
                writer.WritePropertyName("referredSemanticId");
                JsonSerializer.Serialize(writer, value.ReferredSemanticId, options);
            }

            writer.WriteEndObject();
        }

        private sealed record ReferencePayload(ReferenceType Type, IKey[] Keys, IReference ReferredSemanticId)
        {
            public static ReferencePayload Empty { get; } = new(ReferenceType.Undefined, Array.Empty<IKey>(), null);
        }

        private sealed class NonGenericReferenceConverter : JsonConverter<IReference>
        {
            public override IReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var payload = ReadPayload(ref reader, options);
                var reference = new Reference(payload.Keys)
                {
                    Type = payload.Type,
                    ReferredSemanticId = payload.ReferredSemanticId
                };

                return reference;
            }

            public override void Write(Utf8JsonWriter writer, IReference value, JsonSerializerOptions options)
            {
                WriteReference(writer, value, options);
            }
        }

        private sealed class GenericReferenceConverter<TReferable> : JsonConverter<IReference<TReferable>> where TReferable : IReferable
        {
            public override IReference<TReferable> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var payload = ReadPayload(ref reader, options);
                var reference = new Reference<TReferable>(payload.Keys)
                {
                    Type = payload.Type,
                    ReferredSemanticId = payload.ReferredSemanticId
                };

                return reference;
            }

            public override void Write(Utf8JsonWriter writer, IReference<TReferable> value, JsonSerializerOptions options)
            {
                WriteReference(writer, value, options);
            }
        }
    }
}
