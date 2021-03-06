﻿using Duality;
using Pathfindax.Factories;
using Pathfindax.Graph;
using Pathfindax.Nodes;
using System.Collections.Generic;

namespace Pathfindax.Test.Tests.Algorithms
{
    public class AlgorithmTestCases
    {
        public static IEnumerable<object[]> OptimalPathTestCases
        {
            get
            {
                yield return GenerateOptimalPathTestCase(11, 11, new Point2(0, 0), new Point2(10, 0), 10f);
                yield return GenerateOptimalPathTestCase(7, 7, new Point2(0, 0), new Point2(6, 6), 8.49f);
                yield return GenerateOptimalPathTestCase(7, 7, new Point2(0, 0), new Point2(6, 3), 7.24f);
                yield return GenerateOptimalPathTestCase(4, 3, new Point2(3, 0), new Point2(0, 2), 3.83f);
                yield return GenerateOptimalPathTestCase(11, 11, new Point2(0, 0), new Point2(10, 0), 10.83f, new[] { new Point2(5, 0) });
                yield return GenerateOptimalPathTestCase(11, 11, new Point2(0, 0), new Point2(10, 0), 11.66f, new[] { new Point2(5, 0), new Point2(5, 1) });
                yield return GenerateOptimalPathTestCase(11, 11, new Point2(0, 0), new Point2(10, 0), 13.07f, new[] { new Point2(5, 0), new Point2(5, 1), new Point2(5, 2), new Point2(2, 2), new Point2(3, 2), new Point2(3, 1) });
                yield return GenerateOptimalPathTestCase(11, 11, new Point2(0, 0), new Point2(7, 2), 9.24f, new[] { new Point2(5, 0), new Point2(5, 1), new Point2(5, 2), new Point2(2, 2), new Point2(3, 2), new Point2(3, 1) });
            }
        }

        public static IEnumerable<object[]> PossiblePathTestCases
        {
            get
            {
                yield return GeneratePathTestCase(3, 3, new Point2(0, 0), new Point2(2, 2));
                yield return GeneratePathTestCase(3, 3, new Point2(2, 2), new Point2(0, 0));
                yield return GeneratePathTestCase(5, 5, new Point2(2, 2), new Point2(0, 0));
            }
        }

        public static IEnumerable<object[]> NoPossiblePathTestCases
        {
            get
            {
                yield return GeneratePathTestCase(3, 3, new Point2(0, 0), new Point2(2, 2), new[] { new Point2(1, 0), new Point2(1, 1), new Point2(1, 2), });
                yield return GeneratePathTestCase(3, 3, new Point2(2, 2), new Point2(0, 0), new[] { new Point2(1, 0), new Point2(1, 1), new Point2(1, 2), });
                yield return GeneratePathTestCase(5, 5, new Point2(2, 2), new Point2(0, 0), new[] { new Point2(1, 0), new Point2(1, 1), new Point2(1, 2), new Point2(1, 3), new Point2(1, 4), });
            }
        }

        public static IEnumerable<object[]> NodeGridGenerationTestCases
        {
            get
            {
                yield return NodeGridGenerationTestCase(11, 11);
                yield return NodeGridGenerationTestCase(7, 7);
                yield return NodeGridGenerationTestCase(4, 3);
                yield return NodeGridGenerationTestCase(11, 11, new[] { new Point2(5, 0) });
                yield return NodeGridGenerationTestCase(11, 11, new[] { new Point2(5, 0), new Point2(5, 1) });
                yield return NodeGridGenerationTestCase(11, 11, new[] { new Point2(5, 0), new Point2(5, 1), new Point2(5, 2), new Point2(2, 2), new Point2(3, 2), new Point2(3, 1) });
            }
        }

        private static object[] GenerateOptimalPathTestCase(int width, int height, Point2 start, Point2 end, float expectedPathLength, Point2[] blockedNodes = null)
        {
            var factory = new DefinitionNodeGridFactory();
            var collisionMask = new NodeGridCollisionMask(PathfindaxCollisionCategory.Cat1, width, height);
            if (blockedNodes != null)
            {
                foreach (var blockedNode in blockedNodes)
                {
                    collisionMask.Layers[0].CollisionDirections[blockedNode.X, blockedNode.Y] = CollisionDirection.Solid;
                }
            }
            var nodeGrid = factory.GeneratePreFilledArray(GenerateNodeGridConnections.All, collisionMask, true);
            var grid = new DefinitionNodeGrid(nodeGrid, new Vector2(1, 1));


            var description = blockedNodes != null ?
                $"Path from {start} to {end} on a {width} by {height} grid with blocked nodes {string.Join(", ", blockedNodes)}" :
                $"Path from {start} to {end} on a {width} by {height} grid";
            return new object[] { grid, start, end, expectedPathLength };
        }

        private static object[] GeneratePathTestCase(int width, int height, Point2 start, Point2 end, Point2[] blockedNodes = null)
        {
            var factory = new DefinitionNodeGridFactory();
            var collisionMask = new NodeGridCollisionMask(PathfindaxCollisionCategory.Cat1, width, height);
            if (blockedNodes != null)
            {
                foreach (var blockedNode in blockedNodes)
                {
                    collisionMask.Layers[0].CollisionDirections[blockedNode.X, blockedNode.Y] = CollisionDirection.Solid;
                }
            }
            var nodeGrid = factory.GeneratePreFilledArray(GenerateNodeGridConnections.All, collisionMask, true);
            var grid = new DefinitionNodeGrid(nodeGrid, new Vector2(1, 1));


            var description = blockedNodes != null ?
                $"Path from {start} to {end} on a {width} by {height} grid with blocked nodes {string.Join(", ", blockedNodes)}" :
                $"Path from {start} to {end} on a {width} by {height} grid";
            return new object[] { grid, start, end };
        }

        private static object[] NodeGridGenerationTestCase(int width, int height, Point2[] blockedNodes = null)
        {
            var description = blockedNodes != null ?
                $"{width} by {height} grid with blocked nodes {string.Join(", ", blockedNodes)}" :
                $"{width} by {height} grid";
            return new object[] { width, height, blockedNodes ?? new Point2[0] };
        }
    }
}
