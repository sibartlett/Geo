﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Geo.Tests.Gps.Serialization;

public abstract class SerializerTestFixtureBase
{
    protected DirectoryInfo GetReferenceFileDirectory(params string[] subDirectories)
    {
        var filePath = Path.GetDirectoryName(
            new Uri(Assembly.GetExecutingAssembly().Location).LocalPath
        );
        filePath = Path.Combine(filePath, "..", "..");

        var dir = new DirectoryInfo(filePath);
        while (dir != null)
        {
            var refDir = dir.EnumerateDirectories().FirstOrDefault(x => x.Name == "reference");

            if (refDir != null)
            {
                if (subDirectories == null || subDirectories.Length == 0)
                {
                    dir = refDir;
                }
                else
                {
                    foreach (var directory in subDirectories)
                        if (refDir != null)
                            refDir = refDir
                                .EnumerateDirectories()
                                .FirstOrDefault(x => x.Name == directory);
                    dir = refDir;
                }

                break;
            }

            dir = dir.Parent;
        }

        return dir;
    }
}
