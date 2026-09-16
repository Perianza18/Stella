"""Regression tests for Level 6: Galactic Customs."""
from pathlib import Path
import sys
import unittest

LEVEL_DIR = Path(__file__).resolve().parents[1] / "level_06_galactic_customs"
sys.path.insert(0, str(LEVEL_DIR))

from aduana_galactica import resolver_pesada


def apply_equal_three_way_split(candidates: set[int]) -> set[int]:
    ordered = sorted(candidates)
    group_size = len(ordered) // 3
    left = set(ordered[:group_size])
    right = set(ordered[group_size:2 * group_size])
    _, remaining = resolver_pesada(candidates, left, right)
    return remaining


class GalacticCustomsTests(unittest.TestCase):
    def test_equal_three_way_partitions_reduce_27_to_1(self) -> None:
        candidates = set(range(1, 28))
        observed_sizes = []

        for _ in range(3):
            candidates = apply_equal_three_way_split(candidates)
            observed_sizes.append(len(candidates))

        self.assertEqual(observed_sizes, [9, 3, 1])

    def test_unbalanced_partition_retains_largest_subgroup(self) -> None:
        candidates = set(range(1, 28))
        left = set(range(1, 6))
        right = set(range(6, 11))
        outside = candidates - left - right

        _, remaining = resolver_pesada(candidates, left, right)

        largest_subgroup = max(len(left), len(right), len(outside))
        self.assertGreaterEqual(len(remaining), largest_subgroup)
        self.assertEqual(len(remaining), 17)


if __name__ == "__main__":
    unittest.main()
